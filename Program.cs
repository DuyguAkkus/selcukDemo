using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using SelcukDemo.AppDbContext;
using Microsoft.Extensions.Logging;
using SelcukDemo.Services;

var builder = WebApplication.CreateBuilder(args);

// 📌 1️⃣ **Veritabanı Bağlantısını Yapılandır**
builder.Services.AddDbContext<SelcukDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 📌 3️⃣ **Identity (Kullanıcı Yönetimi) Servisini Ekle**
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
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

// 📌 5️⃣ **Bağımlılıkları (DI) Tanımla**
builder.Services.AddScoped<ClaimsService>(); 
builder.Services.AddScoped<UserManager<IdentityUser>>();
builder.Services.AddScoped<RoleManager<IdentityRole>>();
builder.Services.AddScoped<SignInManager<IdentityUser>>();

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

// 📌 9️⃣ **Admin Kullanıcısını Otomatik Ekle (Hatasız Yapılandırıldı)**
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        string adminEmail = "admin@menuproject.com";
        string adminPassword = "Admin@123"; // Güçlü bir şifre belirle

        // **Admin Rolü Yoksa Ekle**
        if (!(await roleManager.RoleExistsAsync("Admin")))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        // **Admin Kullanıcısı Yoksa Ekle**
        var adminUserExists = await userManager.FindByEmailAsync(adminEmail);
        if (adminUserExists == null)
        {
            var adminUser = new IdentityUser
            {
                UserName = "AdminUser",
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                logger.LogInformation("Admin kullanıcı başarıyla oluşturuldu.");
            }
            else
            {
                logger.LogError("Admin kullanıcı oluşturulurken hata oluştu: {Errors}", 
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Admin kullanıcısı eklenirken hata oluştu.");
    }
}

// 📌 🔟 **Middleware (Ara Katmanları) Ayarla**
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// 📌 1️⃣1️⃣ **Varsayılan Route Yapısı**
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// 📌 1️⃣2️⃣ **Admin Paneli için Area Desteği**
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Admin}/{action=Index}/{id?}");

/*using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<SelcukDbContext>();

    await context.Database.MigrateAsync();

    await MenuSeeder.SeedMenus(context);
    await RoleMenusSeeder.SeedRoleMenus(context);
}

*/
app.Run();
