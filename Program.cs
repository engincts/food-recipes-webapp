using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using YemekTarifleri.Db;
using static YemekTarifleri.Controllers.HomeController;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add session services
builder.Services.AddDistributedMemoryCache(); // Adds a default in-memory implementation of IDistributedCache
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
    options.Cookie.HttpOnly = true; // Ensure the session cookie is HTTP only
    options.Cookie.IsEssential = true; // Make the session cookie essential
});
builder.Services.AddDbContext<YemekTarifleriContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));  //Yeni

builder.Services.AddScoped(typeof(YemekTarifleri.Services.IUserManager), typeof(YemekTarifleri.Repository.UserRepository));
builder.Services.AddScoped(typeof(YemekTarifleri.Services.IRecipeManager), typeof(YemekTarifleri.Repository.RecipeRepository));
builder.Services.AddScoped(typeof(YemekTarifleri.Services.ImageManager), typeof(YemekTarifleri.Repository.ImageRepository));
builder.Services.AddScoped(typeof(YemekTarifleri.Services.IRoleManager),typeof(YemekTarifleri.Repository.RoleManager));
//builder.Services.AddScoped(typeof(YemekTarifleri.Services.IIngredientManager), typeof(YemekTarifleri.Repository.IngredientRepository));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Home/AccessDenied";
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    //options.Events=new CookieAuthenticationEvents();
    //{

    //}
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserPolicy", policy => policy.RequireRole("User"));
    options.AddPolicy("ModeratorPolicy", policy => policy.RequireRole("Moderator"));
    // Ýhtiyaç duyduðunuz diðer politikalarý buraya ekleyin
});
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<LocationService>();


// Configure JSON options
builder.Services.AddControllersWithViews().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Shared/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession(); // Ensure this is called after UseRouting and before UseEndpoints

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
