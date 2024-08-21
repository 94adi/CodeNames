namespace CodeNames.Services;

public class SeedDataService : ISeedDataService
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<IdentityUser> _userManager;

    public SeedDataService(RoleManager<IdentityRole> roleManager,
        UserManager<IdentityUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
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
            }

        }, isAdmin: false);
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
