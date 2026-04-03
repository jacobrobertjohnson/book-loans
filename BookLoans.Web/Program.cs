using BookLoans.Abstractions.Interfaces;
using BookLoans.Data;
using BookLoans.Data.Entities;
using BookLoans.Data.Repositories;
using BookLoans.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IAdminAuthorRepository, AdminAuthorRepository>();
builder.Services.AddScoped<IAdminBookRepository, AdminBookRepository>();
builder.Services.AddScoped<IAdminBorrowerRepository, AdminBorrowerRepository>();
builder.Services.AddScoped<IAdminConditionRepository, AdminConditionRepository>();
builder.Services.AddScoped<IAdminCheckoutRepository, AdminCheckoutRepository>();
builder.Services.AddScoped<IPublicHomepageRepository, PublicHomepageRepository>();
builder.Services.AddScoped<IAdminAuthorService, AdminAuthorService>();
builder.Services.AddScoped<IAdminBookService, AdminBookService>();
builder.Services.AddScoped<IAdminBorrowerService, AdminBorrowerService>();
builder.Services.AddScoped<IAdminConditionService, AdminConditionService>();
builder.Services.AddScoped<IAdminCheckoutService, AdminCheckoutService>();
builder.Services.AddScoped<IPublicHomepageQueryService, PublicHomepageQueryService>();
builder.Services
    .AddIdentity<ApplicationUserEntity, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
});

WebApplication app = builder.Build();

await SeedAdminUserAsync(app.Services, app.Configuration);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

static async Task SeedAdminUserAsync(IServiceProvider services, IConfiguration configuration)
{
    using IServiceScope scope = services.CreateScope();
    RoleManager<IdentityRole> roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    UserManager<ApplicationUserEntity> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUserEntity>>();

    const string adminRoleName = "Admin";
    if (!await roleManager.RoleExistsAsync(adminRoleName))
    {
        IdentityResult roleResult = await roleManager.CreateAsync(new IdentityRole(adminRoleName));
        if (!roleResult.Succeeded)
        {
            string roleErrors = string.Join(", ", roleResult.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Failed to create Admin role: {roleErrors}");
        }
    }

    string? adminEmail = configuration["AdminBootstrap:Email"];
    string? adminPassword = configuration["AdminBootstrap:Password"];

    if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
    {
        return;
    }

    ApplicationUserEntity? adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser is null)
    {
        adminUser = new ApplicationUserEntity
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        IdentityResult userResult = await userManager.CreateAsync(adminUser, adminPassword);
        if (!userResult.Succeeded)
        {
            string userErrors = string.Join(", ", userResult.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Failed to create bootstrap admin user: {userErrors}");
        }
    }

    if (!await userManager.IsInRoleAsync(adminUser, adminRoleName))
    {
        IdentityResult roleAssignResult = await userManager.AddToRoleAsync(adminUser, adminRoleName);
        if (!roleAssignResult.Succeeded)
        {
            string assignErrors = string.Join(", ", roleAssignResult.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Failed to assign Admin role: {assignErrors}");
        }
    }
}
