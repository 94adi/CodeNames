using CodeNames.Models;

namespace CodeNames.Services.Seed;

public class SeedDataService : ISeedDataService
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IGameRoomRepository _gameRoomRepo;

    public SeedDataService(RoleManager<IdentityRole> roleManager,
        UserManager<IdentityUser> userManager,
        IGameRoomRepository gameRoomRepo)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _gameRoomRepo = gameRoomRepo;
    }

    public async Task Seed()
    {
        await _AddSeedRoles(new List<IdentityRole>
        {
            new IdentityRole(StaticDetails.ROLE_ADMIN),
            new IdentityRole(StaticDetails.ROLE_USER)
        });

        await _AddSeedUsers(new List<IdentityUser>
        {
            new IdentityUser
            {
                UserName = "admin@admin.com",
                Email = "admin@admin.com",
                EmailConfirmed = true
            }

        }, isAdmin: true);

        await _AddSeedUsers(new List<IdentityUser>
        {
            new IdentityUser
            {
                UserName = "user@user.com",
                Email = "user@user.com",
                EmailConfirmed = true
            },
            new IdentityUser
            {
                UserName = "test1@test.com",
                Email = "test1@test.com",
                EmailConfirmed = true
            },
            new IdentityUser
            {
                UserName = "test2@test.com",
                Email = "test2@test.com",
                EmailConfirmed = true
            },
            new IdentityUser
            {
                UserName = "test3@test.com",
                Email = "test3@test.com",
                EmailConfirmed = true
            }

        }, isAdmin: false);

        _AddGameRooms(new List<GameRoom> { new GameRoom
        {
            Name = "Adi's room",
            InvitationCode =  Guid.NewGuid(),
            MaxNoPlayers = 20
        }
        });
    }

    private void _AddGameRooms(IEnumerable<GameRoom> gameRooms)
    {
        if (gameRooms != null && gameRooms.Count() > 0)
        {
            foreach (var gameRoom in gameRooms)
            {
                _gameRoomRepo.Add(gameRoom);
            }
            _gameRoomRepo.Save();
        }
    }

    private async Task _AddSeedRoles(IEnumerable<IdentityRole> roles)
    {
        foreach (var role in roles)
        {
            bool roleAlreadyExists = await _roleManager.RoleExistsAsync(role.Name);
            if (!roleAlreadyExists)
            {
                await _roleManager.CreateAsync(role);
            }
        }
    }

    private async Task _AddSeedUsers(IEnumerable<IdentityUser> users,
        string password = "$upper@dm1n",
        bool isAdmin = false)
    {
        foreach (var user in users)
        {
            var existingUser = await _userManager.FindByEmailAsync(user.Email);
            bool userAlreadyExists = existingUser != null ? true : false;

            if (!userAlreadyExists)
            {
                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded && isAdmin)
                {
                    await _userManager.AddToRoleAsync(user, StaticDetails.ROLE_ADMIN);
                }
                else if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, StaticDetails.ROLE_USER);
                }
            }
        }
    }
}
