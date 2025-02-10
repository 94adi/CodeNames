using CodeNames.Models;
using Microsoft.Extensions.Options;

namespace CodeNames.Services.Seed;

public class SeedDataService : ISeedDataService
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IGameRoomRepository _gameRoomRepo;
    private readonly UserPasswordSecrets _userPasswordSecrets;
    private readonly SeedDataConfig _seedDataConfig;

    public SeedDataService(RoleManager<IdentityRole> roleManager,
        UserManager<IdentityUser> userManager,
        IGameRoomRepository gameRoomRepo,
        IOptions<UserPasswordSecrets> userPasswordSecretsOptions,
        IOptions<SeedDataConfig> seedDataConfigOptions)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _gameRoomRepo = gameRoomRepo;
        _userPasswordSecrets = userPasswordSecretsOptions.Value;
        _seedDataConfig = seedDataConfigOptions.Value;
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

        }, 
        password:_userPasswordSecrets.Admin, 
        isAdmin: true);

        var testUsers = GenerateTestUsersData();

        await _AddSeedUsers(testUsers,
        password:_userPasswordSecrets.User, 
        isAdmin: false);

        await _AddSeedUsers(new List<IdentityUser>
        {
            new IdentityUser
            {
                UserName = "guest@guest.com",
                Email = "guest@guest.com",
                EmailConfirmed = true
            }

        },
        password: "gU3$tP@@$w0rd",
        isAdmin: false);

        _AddGameRooms(new List<GameRoom> { new GameRoom
        {
            Name = "Adi's room",
            InvitationCode =  Guid.NewGuid(),
            MaxNoPlayers = 20
        },
        new GameRoom
        {
            Name = "Test room",
            InvitationCode = Guid.NewGuid(),
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
        string password,
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

    private List<IdentityUser> GenerateTestUsersData()
    {
        var testUsers = new List<IdentityUser>();

        for (int i = 1; i <= _seedDataConfig.NumberOfTestUsers; i++)
        {
            testUsers.Add(new IdentityUser
            {
                UserName = $"test{i}@test.com",
                Email = $"test{i}@test.com",
                EmailConfirmed = true
            });
        }

        return testUsers;
    }
}
