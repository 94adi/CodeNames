namespace CodeNames.Extensions;

public static class DependencyConfigExtensions
{
    public static void RegisterServices(this WebApplicationBuilder appBuilder)
    {
        var dbConnectionString = appBuilder.Configuration.GetConnectionString("DefaultConnection");

        appBuilder.Services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlite(dbConnectionString));

        appBuilder.Services
            .AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders()
            .AddDefaultUI();

        appBuilder.Services.Configure<IdentityOptions>(opt =>
        {
            opt.Password.RequireDigit = true;
            opt.Password.RequireUppercase = true;
            opt.Password.RequireLowercase = true;
            opt.Password.RequireNonAlphanumeric = false;
            opt.Password.RequiredLength = 10;
            opt.Lockout.MaxFailedAccessAttempts = 3;
            opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            opt.SignIn.RequireConfirmedEmail = true;
        });

        string signalRConnectionString = appBuilder.Configuration["SignalRConfig:ConnectionString"];

        appBuilder.Services.AddSignalR(opt =>
        {
            opt.ClientTimeoutInterval = TimeSpan.FromSeconds(300);
            opt.KeepAliveInterval = TimeSpan.FromSeconds(15);
        })
            .AddAzureSignalR(opt =>
            {
                opt.ConnectionString = signalRConnectionString;
            });

        appBuilder.Services.AddScoped<IGridGenerator, GridGenerator>();
        appBuilder.Services.AddScoped<IGameRoomRepository, GameRoomRepository>();
        appBuilder.Services.AddScoped<IGameRoomService, GameRoomService>();
        appBuilder.Services.AddScoped<IUserRepository, UserRepository>();
        appBuilder.Services.AddScoped<IUserService, UserService>();
        appBuilder.Services.AddScoped<ILiveGameSessionRepository, LiveGameSessionRepository>();
        appBuilder.Services.AddScoped<ILiveGameSessionService, LiveGameSessionService>();
        appBuilder.Services.AddScoped<IStateMachineService, StateMachineService>();
        appBuilder.Services.AddScoped<ISeedDataService, SeedDataService>();
        appBuilder.Services.AddScoped<IDatabaseService, DatabaseService>();
        appBuilder.Services.AddScoped<ISessionService, SessionService>();
        appBuilder.Services.AddScoped<IGameStateService, GameStateService>();
        appBuilder.Services.AddScoped<IPlayerSubmitFactory, PlayerSubmitFactory>();
        appBuilder.Services.AddScoped<PlayerSubmitBlackCardHandler>();
        appBuilder.Services.AddScoped<PlayerSubmitNeutralCardHandler>();
        appBuilder.Services.AddScoped<PlayerSubmitTeamCardHandler>();
        appBuilder.Services.AddScoped<PlayerSubmitOppositeTeamCardHandler>();
        appBuilder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, MyEmailSender>();

        appBuilder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

        appBuilder.Services.AddRazorPages();
    }
}
