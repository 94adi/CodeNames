namespace CodeNames.Extensions;

public static class DatabaseExtensions
{
    public static async Task ApplyMigrations(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var databaseService = scope.ServiceProvider.GetService<IDatabaseService>();
            await databaseService.RunMigrationsAsync();
        }
    }

    public static void DeleteDatabase(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var databaseService = scope.ServiceProvider.GetService<IDatabaseService>();
            databaseService.DeleteDatabase();
        }
    }
}
