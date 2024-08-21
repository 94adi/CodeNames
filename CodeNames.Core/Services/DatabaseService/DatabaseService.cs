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

    public void RunMigrations() => _context.Database.Migrate();

}
