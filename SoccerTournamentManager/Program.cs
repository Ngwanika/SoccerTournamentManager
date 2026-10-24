using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using SoccerTournament.DataAccess.Data;
using SoccerTournament.DataAccess.Repository;
using SoccerTournament.DataAccess.Repository.IRepository;
using SoccerTournament.Models;
using SoccerTournament.Utility;
using SoccerTournamentManager.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});
builder.Services.AddRazorPages();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<EmailSender>();
builder.Services.AddScoped<IEmailSender>(serviceProvider =>
    serviceProvider.GetRequiredService<EmailSender>());
builder.Services.AddScoped<MatchGeneratorService>();
builder.Services.AddScoped<TournamentBracketService>();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (!await dbContext.Database.CanConnectAsync())
    {
        app.Logger.LogWarning(
            "The application database is unavailable. Apply database migrations and verify the connection before using identity features.");
    }
    else
    {
        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
        {
            app.Logger.LogWarning(
                "Found {PendingMigrationCount} pending database migrations. Role initialization was skipped; apply migrations before using identity features.",
                pendingMigrations.Count());
        }
        else
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var roles = new[]
            {
                SD.Role_Admin,
                SD.Role_Customer,
                SD.Role_MatchOfficial,
                SD.Role_TeamManager,
                SD.Role_TournamentManager
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var result = await roleManager.CreateAsync(new IdentityRole(role));
                    if (!result.Succeeded)
                    {
                        throw new InvalidOperationException(
                            $"Unable to create the '{role}' role: {string.Join("; ", result.Errors.Select(error => error.Description))}");
                    }
                }
            }

            var bootstrapEmail = builder.Configuration["BootstrapAdmin:Email"]?.Trim();
            var bootstrapPassword = builder.Configuration["BootstrapAdmin:Password"];
            if (string.IsNullOrWhiteSpace(bootstrapEmail) != string.IsNullOrWhiteSpace(bootstrapPassword))
            {
                throw new InvalidOperationException(
                    "Configure both BootstrapAdmin:Email and BootstrapAdmin:Password to create the initial administrator.");
            }

            if (!string.IsNullOrWhiteSpace(bootstrapEmail))
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var administrators = await userManager.GetUsersInRoleAsync(SD.Role_Admin);
                if (administrators.Count == 0)
                {
                    var bootstrapAdmin = await userManager.FindByEmailAsync(bootstrapEmail);
                    if (bootstrapAdmin == null)
                    {
                        bootstrapAdmin = new ApplicationUser
                        {
                            UserName = bootstrapEmail,
                            Email = bootstrapEmail,
                            EmailConfirmed = true,
                            Name = bootstrapEmail
                        };

                        var createResult = await userManager.CreateAsync(bootstrapAdmin, bootstrapPassword!);
                        if (!createResult.Succeeded)
                        {
                            throw new InvalidOperationException(
                                $"Unable to create the initial administrator: {string.Join("; ", createResult.Errors.Select(error => error.Description))}");
                        }
                    }

                    var assignResult = await userManager.AddToRoleAsync(bootstrapAdmin, SD.Role_Admin);
                    if (!assignResult.Succeeded)
                    {
                        throw new InvalidOperationException(
                            $"Unable to assign the initial Admin role: {string.Join("; ", assignResult.Errors.Select(error => error.Description))}");
                    }

                    app.Logger.LogInformation("The initial Admin account has been provisioned.");
                }
            }
        }
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Customer/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

app.Run();
