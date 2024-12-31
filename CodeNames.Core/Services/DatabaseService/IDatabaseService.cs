namespace CodeNames.Core.Services.DatabaseService;

public interface IDatabaseService
{
    void RunMigrations();

    void DeleteDatabase();
}
