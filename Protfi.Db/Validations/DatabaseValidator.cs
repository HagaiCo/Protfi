using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Protfi.Common;
using Protfi.Common.Reflection;

namespace Protfi.Db.Validations;

public class DatabaseValidator
{
    private HashSet<string> DatabaseExistingChecked { get; } = new();

    private void ValidateDatabases(DbContext context)
    {
        Guard.NotNull(context, nameof(context));

        var key = $"Global\\{context.GetType().FullName}";
        if (DatabaseExistingChecked.Contains(key))
            return;

        var mutex = new Mutex(false, key);
        try
        {
            mutex.WaitOne();
            context.Database.Migrate();

            var connection = (NpgsqlConnection)context.Database.GetDbConnection();
            connection.Open();
            connection.ReloadTypes();
            connection.Close();
        }
        finally
        {
            mutex.ReleaseMutex();
            mutex.Dispose();
        }
    }

    private void ValidateDatabases(IEnumerable<DbContext> contexts)
    {
        Guard.NotNull(contexts, nameof(contexts));

        foreach (var context in contexts)
        {
            ValidateDatabases(context);
        }
    }
    
    public void ValidateDatabases(bool createIfNotExist=false)
    {
        ICollection<PropertyInfo> GetProperties(Type dbContextType) =>
            dbContextType.GetProperties()
                .Where(p => p.PropertyType.IsGenericType &&
                            p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                .ToList();

        var assemblyScanner = new AssemblyScanner();
        var items = (
            from dbContextType in assemblyScanner.GetChildrenOfType<DbContext>(typeof(ProtfiDbContextAttribute))
            where !dbContextType.IsAbstract
            let props = GetProperties(dbContextType)
            where props.Any()
            let context = (DbContext) Activator.CreateInstance(dbContextType)
            select new { context, props }
        ).ToList();

        if (createIfNotExist)
        {
            var contexts = items.Select(item => item.context).ToList();
            ValidateDatabases(contexts);
        }

        var modelErrors = new List<string>();
        foreach (var item in items)
        {
            if (item.context.Database.GetPendingMigrations().Any())
            {
                modelErrors.Add($"Database '{item.context.GetDbName()}' has pending migrations.");
            }
            else
            {
                try
                {
                    foreach (var propInf in item.props)
                    {
                        var propObj = propInf.GetValue(item.context);
                        var entityType = item.context.Model.FindEntityType(propObj.GetType().GetGenericArguments()[0]);
                        var skip = entityType.FindPrimaryKey() == null || entityType.IsOwned();
                        if (skip)
                        {
                            continue;
                        }
                        
                        var localPropObj = propObj.GetType().GetProperty("Local").GetValue(propObj);
                        localPropObj.GetType().GetProperty("Count").GetValue(localPropObj);
                    }
                }
                catch (TargetInvocationException ex) when (ex.InnerException is InvalidOperationException)
                {
                    modelErrors.Add(item.context.GetDbName());
                }
            }
        }

        items.ForEach(v => v.context.Dispose());

        if (modelErrors.Any())
        {
            throw new ValidationException(string.Join(", ", modelErrors));
        }
    }
}