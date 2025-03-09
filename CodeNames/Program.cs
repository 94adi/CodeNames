var builder = WebApplication.CreateBuilder(args);

builder.Services.RegisterConfigs(builder);

builder.RegisterServices();

builder.WebHost.UseStaticWebAssets();

bool conversionResult = bool.TryParse(builder.Configuration["DatabaseSettings:DeleteOnStartup"],
    out bool deleteOnStartup);

bool databaseDeletionCondition = conversionResult && deleteOnStartup;

var app = builder.Build();

if (databaseDeletionCondition)
{
    app.DeleteDatabase();
}

app.ApplyMigrations().GetAwaiter().GetResult();
app.SeedData().GetAwaiter().GetResult();

if (app.Environment.IsAzureEnv())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseCors("AllowAll");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "MyArea",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<StateMachineHub>("/hubs/stateMachineHub");

app.MapRazorPages();

app.Run();