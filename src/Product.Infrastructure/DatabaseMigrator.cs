using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Product.Infrastructure.Persistence;

namespace Product.Infrastructure;

public static class DatabaseMigrator
{
    public static async Task MigrateDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ProductDbContext>>();

        var pending = (await db.Database.GetPendingMigrationsAsync()).ToList();
        if (pending.Count == 0)
        {
            logger.LogInformation("Database is up to date. No migrations to apply.");
            return;
        }

        logger.LogInformation("Applying {Count} pending migration(s): {Migrations}",
            pending.Count, string.Join(", ", pending));

        await db.Database.MigrateAsync();

        logger.LogInformation("Database migration completed successfully.");
    }
}
