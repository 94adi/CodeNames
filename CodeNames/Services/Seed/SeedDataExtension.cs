using CodeNames.Core.Services.DatabaseService;

namespace CodeNames.Services.Seed;

public static class SeedDataExtension
{
    public static async Task SeedData(this WebApplication app)
    {
            using (var scope = app.Services.CreateScope())
            {
                var seedService = scope.ServiceProvider.GetService<ISeedDataService>();
                await seedService.Seed();
            }
    }
}
