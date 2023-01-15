using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Protfi.Common.Extensions;
using Protfi.Common.Helpers;

namespace Protfi.Common.EF
{
    public static class EfExtensions
    {
        private static readonly MethodInfo StringToUpper = typeof(string).GetMethod("ToUpper", new Type[0]);
		private static readonly MethodInfo CollectionContains = typeof(ICollection<string>).GetMethod("Contains", new Type[] { typeof(string) });
		private static readonly MethodInfo StringContains = typeof(string).GetMethod("Contains", new Type[] { typeof(string) });

        [Obsolete("Was used as a workaround for EF31")]
        //TODO all places that use this method should be considered for refactoring after update to EF5, Npgsql5
        public static ICollection<T> RemoveEntitiesWhereOnClient<T>(this DbSet<T> @this, Func<T, bool> filter) where T : class
        {
            var entitiesToRemove = @this.ToList().Where(filter).ToList();

            @this.RemoveRange(entitiesToRemove);

            return entitiesToRemove;
        }

        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> query, string propertyName, SortOrder direction)
        {
            return direction == SortOrder.Ascending ? query.OrderBy(propertyName) : query.OrderByDescending(propertyName);
        }

        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> query, string propertyName)
        {
            return CallOrderedQueryable(query, "OrderBy", propertyName);
        }

        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> query, string propertyName)
        {
            return CallOrderedQueryable(query, "OrderByDescending", propertyName);
        }

        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> query, string propertyName)
        {
            return CallOrderedQueryable(query, "ThenBy", propertyName);
        }

        public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> query, string propertyName)
        {
            return CallOrderedQueryable(query, "ThenByDescending", propertyName);
        }

        private static IOrderedQueryable<TEntity> CallOrderedQueryable<TEntity>(this IQueryable<TEntity> query, string methodName, string propertyName)
        {
            Guard.NotNull(() => query, query);
            Guard.NotNullOrEmpty(() => methodName, methodName);
            Guard.NotNullOrEmpty(() => propertyName, propertyName);

            var param = Expression.Parameter(typeof(TEntity), "entity");
            var propertyPath = propertyName.Split('.').Aggregate<string, Expression>(param, Expression.PropertyOrField);
            var lambda = Expression.Lambda(propertyPath, param);
            var call = Expression.Call(typeof(Queryable), methodName, new[] { typeof(TEntity), propertyPath.Type }, query.Expression, lambda);
            return (IOrderedQueryable<TEntity>)query.Provider.CreateQuery(call);
        }

        public static IQueryable<TEntity> WhereIn<TEntity, T1, T2>(this IQueryable<TEntity> query,
            Expression<Func<TEntity, T1>> firstProperty,
            Expression<Func<TEntity, T2>> secondProperty,
            ICollection<Tuple<T1, T2>> values)
        {
            Guard.NotNull(query, nameof(query));
            Guard.NotNull(firstProperty, nameof(firstProperty));
            Guard.NotNull(secondProperty, nameof(secondProperty));
            Guard.NotNull(values, nameof(values));

            if (values.Count == 0)
            {
                return query.Where(item => false);
            }

            var parameter = secondProperty.Parameters[0];
            var parametersMap = new Dictionary<ParameterExpression, ParameterExpression> { { firstProperty.Parameters[0], parameter } };
            //firstProperty = ParameterRebinder.ReplaceParameters(firstProperty, parametersMap);
            var filter = (
                from item in values
                let first = Expression.Equal(firstProperty.Body, Expression.Constant(item.Item1, typeof(T1)))
                let second = Expression.Equal(secondProperty.Body, Expression.Constant(item.Item2, typeof(T2)))
                select Expression.And(first, second)
            ).Aggregate(Expression.Or);

            var lambda = Expression.Lambda<Func<TEntity, bool>>(filter, parameter);
            return query.Where(lambda);
        }
        
        public static IQueryable<TEntity> WhereInIgnoreCase<TEntity>(this IQueryable<TEntity> query, Expression<Func<TEntity, string>> selector, ICollection<string> values)
        {
            Guard.NotNull(() => query, query);
            Guard.NotNull(() => selector, selector);
            Guard.NotNull(() => values, values);


            if (values.Count == 1)
            {
                return WhereEqualsIgnoreCase(query, selector, values.First());
            }

            var lambda = BuildWhereInIgnoreCase(selector, values);
            return query.Where(lambda);
        }

        public static IQueryable<TEntity> WhereEqualsIgnoreCase<TEntity>(this IQueryable<TEntity> query, Expression<Func<TEntity, string>> selector, string value, bool nullIsEmpty = false)
        {
            Guard.NotNull(() => query, query);
            Guard.NotNull(() => selector, selector);

            var property = nullIsEmpty ? Expression.Coalesce(selector.Body, Expression.Constant(String.Empty)) : selector.Body;
            var wrappedSelector = value != null ? Expression.Call(property, StringToUpper) : property;
            var upperValue = nullIsEmpty ? value?.ToUpper() ?? String.Empty : value?.ToUpper();
            var condition = Expression.Equal(wrappedSelector, Expression.Constant(upperValue));
            var lambda = Expression.Lambda<Func<TEntity, bool>>(condition, selector.Parameters);
            return query.Where(lambda);
        }

        public static Expression<Func<TEntity, bool>> BuildWhereInIgnoreCase<TEntity>(Expression<Func<TEntity, string>> selector, ICollection<string> values)
        {
            if (values == null)
            {
                return item => true;
            }

            var upperValues = values.Select(item => item != null ? item.ToUpper() : item).ToList();
            var valuesContainer = new { upperValues };//for avoiding caching this expression inside EF Core
            var accessor = valuesContainer.GetType().GetProperty(nameof(valuesContainer.upperValues));
            var wrappedSelector = Expression.Call(selector.Body, StringToUpper);
            var condition = Expression.Call(Expression.Property(Expression.Constant(valuesContainer), accessor), CollectionContains, wrappedSelector);
            var lambda = Expression.Lambda<Func<TEntity, bool>>(condition, selector.Parameters);
            return lambda;
        }

        public static IQueryable<TEntity> ApplySearch<TEntity>(this IQueryable<TEntity> query, 
            string term,
            params Expression<Func<TEntity, string>>[] properties)
        {
            Guard.NotNull(query, nameof(query));
            
            if (String.IsNullOrEmpty(term) || properties.Length < 1)
            {
                return query;
            }

            var entity = Expression.Parameter(typeof(TEntity));
            var termExp = Expression.Constant(term);
            var filter = properties
                .Select(PropertyNameHelper.GetPropertyInfo)
                .Select(v => Expression.Property(entity, v))
                .Select(v => Expression.Call(v, StringContains, termExp))
                .Cast<Expression>()
                .Aggregate(Expression.Or);

            var lambda = Expression.Lambda<Func<TEntity, bool>>(filter, entity);
            return query.Where(lambda);
        }
        
        public static IQueryable<TEntity> WhereOptional<TEntity>(this IQueryable<TEntity> query, Expression<Func<TEntity, bool>> filter = null)
        {
            return filter != null ? query.Where(filter) : query;
        }
        
        public static IQueryable<TEntity> WhereIf<TEntity>(this IQueryable<TEntity> query, bool condition, Expression<Func<TEntity, bool>> filter)
        {
            return condition ? query.Where(filter) : query;
        }
        
        public static IQueryable<TEntity> TransformOptional<TEntity>(this IQueryable<TEntity> query, Func<IQueryable<TEntity>, IQueryable<TEntity>> transform = null)
        {
            return transform?.Invoke(query) ?? query;
        }

        #region SavingChanges
        
        public static void SetModificationTime(this ChangeTracker tracker, List<EntityEntry> entries)
        {
            var now = DateTime.UtcNow.ToUnixTime();
            var created = tracker.EntityWithStateAndType<ICreatableEntity>(entries, EntityState.Added).ToList();

            foreach (var entity in created)
            {
                entity.typed.CreationTimeUnixTimeInMs = now;
            }

            var modified = tracker.EntityWithStateAndType<IModifiableEntity>(entries, EntityState.Modified);
            var createdAsModified = created.Select(item => item.entry.Entity)
                .OfType<IModifiableEntity>();
            var allModified = modified.Select(item => item.typed)
                .Concat(createdAsModified);

            foreach (var entity in allModified)
            {
                entity.ModificationTimeUnixTimeInMs = now;
            }
        }

        private static IEnumerable<(EntityEntry entry, TEntity typed)> EntityWithStateAndType<TEntity>(this ChangeTracker tracker, IEnumerable<EntityEntry> entityEntries, EntityState state) where TEntity : class
        {
            return from entry in entityEntries
                   where entry.State == state
                   let typed = entry.Entity as TEntity
                   where typed != null
                   select (entry, typed);
        }

        #endregion SavingChanges

        #region ModelBuilder

        public static void SetDefaultTypeFor(this ModelBuilder modelBuilder, Type propertyType, string typeName)
        {
            Guard.NotNull(modelBuilder, nameof(modelBuilder));
            Guard.NotNull(propertyType, nameof(propertyType));

            bool TypeIsNotDefined(IMutableProperty property)
            {
                var column = property.PropertyInfo
                    ?.GetCustomAttributes(false)
                    .OfType<ColumnAttribute>()
                    .FirstOrDefault();
                return String.IsNullOrEmpty(column?.TypeName);
            }

            var properties =
                from entityType in modelBuilder.Model.GetEntityTypes()
                where !entityType.IsKeyless || entityType.IsOwned()
                from property in entityType.GetProperties()
                where property.ClrType == propertyType && TypeIsNotDefined(property)
                select new { property, entityType };

            foreach (var item in properties)
            {
                modelBuilder.Entity(
                    item.entityType.ClrType,
                    builder => builder.Property(item.property.ClrType, item.property.Name).HasColumnType(typeName)
                );
            }
        }

        #endregion ModelBuilder
    }
}