using Bank.Service.DataModels;
using Microsoft.EntityFrameworkCore;
using Protfi.Common;
using Protfi.Common.Db;

namespace Bank.Service;

[ProtfiDbContext]
public class BankDbContext : SqlServerDbContext
{
    protected override string DbName => "banks_db";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BankModel>()
            .HasKey(model => model.Id);
        
        modelBuilder.Entity<AccountInformationModel>()
            .HasKey(model => model.Id);
    }
    
    public DbSet<BankModel> Banks { get; set; }

    public DbSet<AccountInformationModel> Accounts { get; set; }
}