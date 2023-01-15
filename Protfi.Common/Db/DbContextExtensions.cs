using Microsoft.EntityFrameworkCore;

namespace Protfi.Common;

public static class DbContextExtensions
{
    public static string GetDbName(this DbContext context)
    {
        return context.Database.GetDbConnection().Database;
    }
}