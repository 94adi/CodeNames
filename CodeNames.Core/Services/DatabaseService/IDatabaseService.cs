namespace CodeNames.Core.Services.DatabaseService;

public interface IDatabaseService
{
    Task RunMigrationsAsync();

    void DeleteDatabase();
}
