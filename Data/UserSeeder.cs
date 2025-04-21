using Microsoft.AspNetCore.Identity;
using SelcukDemo.Models;
using System.Threading.Tasks;

public static class UserSeeder
{
    public static async Task SeedUsers(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        // Rolleri oluştur (eğer yoksa)
        string[] roles = { "Admin", "Teacher", "Student" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // ✅ Admin Kullanıcı
        if (await userManager.FindByEmailAsync("admin@site.com") == null)
        {
            var admin = new AppUser
            {
                UserName = "admin",
                Email = "admin@site.com",
                EmailConfirmed = true,
                BirthDate = new DateTime(1990, 1, 1),
                Gender = "Kadın",
                City = "Konya"
            };

            var result = await userManager.CreateAsync(admin, "Duygu536*");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "Admin");
        }

        // ✅ Öğretmen Kullanıcı
        if (await userManager.FindByEmailAsync("teacher@site.com") == null)
        {
            var teacher = new AppUser
            {
                UserName = "teacher",
                Email = "teacher@site.com",
                EmailConfirmed = true,
                BirthDate = new DateTime(1995, 5, 5),
                Gender = "Erkek",
                City = "Ankara"
            };

            var result = await userManager.CreateAsync(teacher, "Duygu536*");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(teacher, "Teacher");
        }

        // ✅ Öğrenci Kullanıcı
        if (await userManager.FindByEmailAsync("student@site.com") == null)
        {
            var student = new AppUser
            {
                UserName = "student",
                Email = "student@site.com",
                EmailConfirmed = true,
                BirthDate = new DateTime(2003, 9, 15),
                Gender = "Kadın",
                City = "İstanbul"
            };

            var result = await userManager.CreateAsync(student, "Duygu536*");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(student, "Student");
        }
    }
}
