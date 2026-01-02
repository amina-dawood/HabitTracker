using HabitTracker;
using HabitTracker.Data;
using HabitTracker.Models.Interface;
using HabitTracker.Models.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// Add services to container
builder.Services.AddLogging();

var connectionString = builder.Configuration["ConnectionStrings:DefaultConnection"];

// Database setup
builder.Services.AddDbContext<HabitTrackerContext>(options =>
    options.UseSqlServer(connectionString));

// Identity setup (for user authentication)
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<HabitTrackerContext>()
.AddDefaultTokenProviders()
.AddClaimsPrincipalFactory<CustomClaimsPrincipalFactory>();
// Configure application cookies
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});

// Add repositories
builder.Services.AddScoped<IHabitRepository, HabitRepository>();

// Add services
builder.Services.AddScoped<IHabitService, HabitTracker.Services.HabitService>();

// Add role manager
builder.Services.AddScoped<RoleManager<IdentityRole>>();

    // Add authorization policies
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", policy =>
            policy.RequireRole("Admin"));

        options.AddPolicy("CanCreateHabits", policy =>
            policy.RequireClaim("Permission", "CanCreateHabits"));

        options.AddPolicy("CanEditHabits", policy =>
            policy.RequireClaim("Permission", "CanEditOwnHabits"));

        options.AddPolicy("CanDeleteHabits", policy =>
            policy.RequireClaim("Permission", "CanDeleteOwnHabits"));

        options.AddPolicy("HabitOwner", policy =>
            policy.Requirements.Add(new HabitOwnerRequirement()));
    });

// Register the authorization handler
builder.Services.AddTransient<IAuthorizationHandler, HabitOwnerRequirementHandler>();

// Add controllers with views
builder.Services.AddControllersWithViews();

// Add Razor Pages (required for Identity)
builder.Services.AddRazorPages();

// Add SignalR
builder.Services.AddSignalR();

var app = builder.Build();

// Seed admin user (for development only)
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    // Create admin role if it doesn't exist
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    // Create admin user if it doesn't exist
    var adminEmail = "admin@habittracker.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, "Admin123!");
        if (result.Succeeded)
        {
            var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
            Console.WriteLine($"Admin user created successfully. Role added: {roleResult.Succeeded}");
        }
        else
        {
            Console.WriteLine($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
    else
    {
        // Ensure existing admin user has the role
        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
            Console.WriteLine($"Admin role added to existing user: {roleResult.Succeeded}");
        }
        else
        {
            Console.WriteLine("Admin user already exists with Admin role");
        }
    }
}

// Configure pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Global error handling middleware
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An unhandled exception occurred while processing request {RequestPath}", context.Request.Path);

        // For AJAX requests, return a JSON error
        if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            await context.Response.Body.WriteAsync(System.Text.Encoding.UTF8.GetBytes("{\"success\":false,\"message\":\"An unexpected error occurred. Please try again.\"}"));
            return;
        }

        // For regular requests, let the default error handler take care of it
        throw;
    }
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// Map SignalR hub
app.MapHub<HabitTracker.Hubs.HabitHub>("/habitHub");
app.Run();