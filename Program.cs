using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using SelcukDemo.AppDbContext;
using Microsoft.Extensions.Logging;
using SelcukDemo.Filters;
using SelcukDemo.Models;
using SelcukDemo.Services;

var builder = WebApplication.CreateBuilder(args);

// 📌 1️⃣ **Veritabanı Bağlantısını Yapılandır**
builder.Services.AddDbContext<SelcukDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 📌 3️⃣ **Identity (Kullanıcı Yönetimi) Servisini Ekle**
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false; // Email doğrulamasız giriş
    })
    .AddEntityFrameworkStores<SelcukDbContext>()
    .AddDefaultTokenProviders();

// 📌 4️⃣ **Yetkilendirme Politikalarını Ekle**
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireTeacherRole", policy => policy.RequireRole("Teacher"));
    options.AddPolicy("RequireStudentRole", policy => policy.RequireRole("Student"));
});

builder.Services.AddScoped<RequireCompleteProfileAttribute>();


// 📌 5️⃣ **Bağımlılıkları (DI) Tanımla**
builder.Services.AddScoped<ClaimsService>();
builder.Services.AddHttpContextAccessor(); // Eğer başka yerlerde ihtiyaç varsa
builder.Services.AddScoped<UserManager<AppUser>>();
builder.Services.AddScoped<RoleManager<IdentityRole>>();

// 📌 6️⃣ **Kimlik Doğrulama ve Cookie Ayarları**
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/SignIn"; 
    options.AccessDeniedPath = "/Account/AccessDenied"; 
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(60);
    options.SlidingExpiration = true;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/Account/SignIn";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

// 📌 7️⃣ **Dosya Sağlayıcıyı Tanımla**
builder.Services.AddSingleton<IFileProvider>(
    new PhysicalFileProvider(Directory.GetCurrentDirectory()));

// 📌 8️⃣ **MVC Servisini ve View Desteğini Ekle**
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ClaimsService>();
var app = builder.Build();


// 📌 🔟 **Middleware (Ara Katmanları) Ayarla**
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ✅ Statik dosyaların düzgün sunulabilmesi için
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")
    ),
    RequestPath = ""
});

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"); // AREA ROUTE 💥

    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    await UserSeeder.SeedUsers(userManager, roleManager);
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<SelcukDbContext>();
    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    
    await MenuSeeder.SeedMenus(context); // önce bu
    await RoleMenusSeeder.SeedRoleMenus(context); // sonra bu
    await UserSeeder.SeedUsers(userManager, roleManager); // en son

}

app.Run();
