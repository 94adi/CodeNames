using CodeNames.Core.Services.DatabaseService;

namespace CodeNames.Services.Seed;

public static class SeedDataExtension
{
    public static async Task SeedData(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            using (var scope = app.Services.CreateScope())
            {
                var databaseService = scope.ServiceProvider.GetService<IDatabaseService>();
                var seedService = scope.ServiceProvider.GetService<ISeedDataService>();


                databaseService.DeleteDatabase();
                databaseService.RunMigrations();
                await seedService.Seed();
            }

        }
    }
}
