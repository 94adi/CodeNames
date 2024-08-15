var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var dbConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.Configure<GameParametersOptions>(builder.Configuration.GetSection("GameVariables"));


builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(dbConnectionString));

builder.Services.AddDefaultIdentity<IdentityUser>(
    opt => opt.SignIn.RequireConfirmedAccount = true
    ).AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddSignalR();
    //.AddAzureSignalR(connectionAzureSignalR);

builder.Services.AddScoped<IGridGenerator, GridGenerator>();
builder.Services.AddScoped<IGameRoomRepository, GameRoomRepository>();
builder.Services.AddScoped<IGameRoomService, GameRoomService>();
builder.Services.AddScoped<IUserRepository,  UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ILiveGameSessionRepository, LiveGameSessionRepository>();
builder.Services.AddScoped<ILiveGameSessionService, LiveGameSessionService>();
builder.Services.AddScoped<IStateMachineService, StateMachineService>();

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "MyArea",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.MapHub<StateMachineHub>("/hubs/stateMachineHub");
//app.MapHub<LiveSessionHub>("/hubs/liveSession");

app.Run();