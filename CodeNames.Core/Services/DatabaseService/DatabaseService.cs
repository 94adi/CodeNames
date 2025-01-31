using CodeNames.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeNames.Core.Services.DatabaseService;

public class DatabaseService : IDatabaseService
{
    private readonly AppDbContext _context;

    public DatabaseService(AppDbContext context)
    {
        _context = context;
    }

    public void DeleteDatabase() => _context.Database.EnsureDeleted();

    public async Task RunMigrationsAsync()
    {
        if (_context.Database.GetPendingMigrations().Count() > 0)
        {
            await _context.Database.MigrateAsync();
        }
    }

}
