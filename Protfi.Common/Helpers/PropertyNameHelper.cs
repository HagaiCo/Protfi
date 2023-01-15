using System.Linq.Expressions;
using System.Reflection;

namespace Protfi.Common.Helpers
{
    public static class PropertyNameHelper
    {
        public static string GetPropertyName<T>(Expression<Func<T>> propertyLambda)
        {
            var me = propertyLambda.Body as MemberExpression;

            if (me == null)
            {
                throw new ArgumentException("You must pass a lambda of the form: '() => Class.Property' or '() => object.Property'");
            }

            return me.Member.Name;
        }

        public static PropertyInfo GetPropertyInfo(Expression<Func<object>> propertyLambda)
        {
            Guard.NotNull(() => propertyLambda, propertyLambda);

            return GetPropertyInfoFromBody(propertyLambda.Body);
        }
        
        public static PropertyInfo GetPropertyInfo<TItem>(Expression<Func<TItem, object>> propertyLambda)
        {
            Guard.NotNull(() => propertyLambda, propertyLambda);

            return GetPropertyInfoFromBody(propertyLambda.Body);
        }

        public static string GetPropertyName(Expression<Func<object>> propertyLambda)
        {
            var info = GetPropertyInfo(propertyLambda);
            return info.Name;
        }

        public static string GetPropertyName<T, TReturn>(Expression<Func<T, TReturn>> expression)
        {
            Guard.NotNull(() => expression, expression);

            var property = GetPropertyInfo(expression);
            return property.Name;
        }
        
        public static PropertyInfo GetPropertyInfo<T, TReturn>(Expression<Func<T, TReturn>> expression)
        {
            Guard.NotNull(() => expression, expression);

            PropertyInfo Extract(Expression item)
            {
                while (true)
                {
                    if (item is UnaryExpression unary)
                    {
                        item = unary.Operand;
                        continue;
                    }

                    if (item is MemberExpression member)
                    {
                        return (PropertyInfo) member.Member;
                    }

                    var message = $"Can't extract property name from {expression}.";
                    throw new ArgumentException(message);
                    break;
                }
            }

            return Extract(expression.Body);
        }
        
        private static PropertyInfo GetPropertyInfoFromBody(Expression item)
        {
            while (true)
            {
                if (item is UnaryExpression unary)
                {
                    item = unary.Operand;
                    continue;
                }

                if (item is MemberExpression member && member.Member is PropertyInfo info)
                {
                    return info;
                }

                var message = $"Can't extract property name from {item}.";
                throw new ArgumentException(message);
            }
        }
    }
}
