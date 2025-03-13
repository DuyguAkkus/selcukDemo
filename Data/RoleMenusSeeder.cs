using Microsoft.EntityFrameworkCore;
using SelcukDemo.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SelcukDemo.AppDbContext;

public static class RoleMenusSeeder
{
    public static async Task SeedRoleMenus(SelcukDbContext context)
    {
        if (!await context.RoleMenus.AnyAsync()) // Eğer tablo boşsa veri ekle
        {
            // 📌 Menülerin ID'lerini veritabanından alalım
            var anasayfa = await context.UserMenus.FirstOrDefaultAsync(m => m.Name == "Anasayfa");
            var derslerim = await context.UserMenus.FirstOrDefaultAsync(m => m.Name == "Derslerim");
            var menuIslemleri = await context.UserMenus.FirstOrDefaultAsync(m => m.Name == "Menü İşlemleri");
            var menuSil = await context.UserMenus.FirstOrDefaultAsync(m => m.Name == "Menü Sil");
            var dersIslemleri = await context.UserMenus.FirstOrDefaultAsync(m => m.Name == "Ders İşlemleri");
            var dersDuzenle = await context.UserMenus.FirstOrDefaultAsync(m => m.Name == "Ders Düzenle");

            var roleMenus = new List<RoleMenu>
            {
                new RoleMenu { RoleName = "Admin", MenuId = anasayfa?.Id ?? 0 },
                new RoleMenu { RoleName = "Admin", MenuId = menuIslemleri?.Id ?? 0 },
                new RoleMenu { RoleName = "Admin", MenuId = menuSil?.Id ?? 0 },
                new RoleMenu { RoleName = "Student", MenuId = anasayfa?.Id ?? 0 },
                new RoleMenu { RoleName = "Student", MenuId = derslerim?.Id ?? 0 },
                new RoleMenu { RoleName = "Teacher", MenuId = anasayfa?.Id ?? 0 },
                new RoleMenu { RoleName = "Teacher", MenuId = derslerim?.Id ?? 0 },
                new RoleMenu { RoleName = "Admin", MenuId = dersIslemleri?.Id ?? 0 },
                new RoleMenu { RoleName = "Student", MenuId = dersDuzenle?.Id ?? 0 }
            };

            await context.RoleMenus.AddRangeAsync(roleMenus);
            await context.SaveChangesAsync();
        }
    }
}
