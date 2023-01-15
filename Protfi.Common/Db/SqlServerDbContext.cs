using Microsoft.EntityFrameworkCore;
using Npgsql;
using Protfi.Common.EF;
using Z.EntityFramework.Plus;

namespace Protfi.Common.Db
{
    public abstract class SqlServerDbContext : DbContext
    {
        
        static SqlServerDbContext()
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            // use only the free features of the library (free for commercial use too!)
            EntityFrameworkPlusManager.IsCommunity = true;
            NpgsqlConnection.GlobalTypeMapper.UseJsonNet();
        }

        protected SqlServerDbContext() { }
        
        private SqlServerDbContext(DbContextOptions<SqlServerDbContext> options) : base(options) { }
        
        private SqlServerDbContext(bool @readonly)
        {
            if (@readonly)
            {
                ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            }
        }

        protected abstract string DbName { get; }
        private bool DisableDataModelTimeUpdates { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            try
            {
                // var factory = IoC.ResolveOptional<ILoggerFactory>();
                // if (factory != null)
                //     optionsBuilder.UseLoggerFactory(factory);
            }
            catch
            {
                // prevent failing when instantiated by external tools (ef-tools, DbInitializer)
            }
            
            var connectionString = ConnectionStringService.GetConnectionString(DbName);
            optionsBuilder.UseNpgsql(connectionString, options =>
            {
                options.MigrationsAssembly(this.GetType().Assembly.FullName);
            });
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
           
            modelBuilder.UseSerialColumns();
            modelBuilder.UsePropertyAccessMode(PropertyAccessMode.Property);

            modelBuilder.HasPostgresExtension(PostgresSqlDefaultTypes.CiTextExtension);
            modelBuilder.SetDefaultTypeFor(typeof(string), PostgresSqlDefaultTypes.CiText);
            
            var types = modelBuilder.Model.GetEntityTypes().Where(v => !v.IsOwned());
            foreach (var entityType in types)
            {
                entityType.SetTableName(entityType.ClrType.Name);
            }
        }

        #region SaveChanges

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            var allEntities = ChangeTracker.Entries().ToList();
            if (!DisableDataModelTimeUpdates)
            {
                ChangeTracker.SetModificationTime(allEntities);
            }
            
            var result = base.SaveChanges(acceptAllChangesOnSuccess);
            return result;
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            var allEntities = ChangeTracker.Entries().ToList();

            if (!DisableDataModelTimeUpdates)
            {
                ChangeTracker.SetModificationTime(allEntities);
            }
            
            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            return result;
        }

        #endregion SaveChanges
    }
}
