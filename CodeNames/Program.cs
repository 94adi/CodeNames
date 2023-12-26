using CodeNames;
using CodeNames.CodeNames.Core.Services.GridGenerator;
using CodeNames.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var dbConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
/*TO DO: Use Options pattern*/
int col;
int row;
var colConversion = Int32.TryParse(builder.Configuration.GetSection("GameVariables")["col"], out col);
col = colConversion ? col : 5;

var rowConversion = Int32.TryParse(builder.Configuration.GetSection("GameVariables")["col"], out row);
row = rowConversion ? row : 5;


builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(dbConnectionString));

builder.Services.AddDefaultIdentity<IdentityUser>(
    opt => opt.SignIn.RequireConfirmedAccount = true
    ).AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddScoped<IGridGenerator>(o => new GridGenerator(col,row));

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
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.Run();