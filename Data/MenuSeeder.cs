using Microsoft.EntityFrameworkCore;
using SelcukDemo.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SelcukDemo.AppDbContext;

public static class MenuSeeder
{
    public static async Task SeedMenus(SelcukDbContext context)
    {
        if (!await context.UserMenus.AnyAsync()) // Eğer tablo boşsa veri ekle
        {
            // 📌 Önce ANA menüleri ekleyelim
            var mainMenus = new List<UserMenu>
            {
                new UserMenu { Name = "Anasayfa", ParentId = null, ControllerName = "Home", ActionName = "Dashboard", Icon = "fa-solid fa-home", SortNumber = 1, IsVisible = true },
                new UserMenu { Name = "Derslerim", ParentId = null, ControllerName = "Student", ActionName = "Courses", Icon = "fa-solid fa-book", SortNumber = 2, IsVisible = true },
                new UserMenu { Name = "Form İşlemleri", ParentId = null, ControllerName = "Admin", ActionName = "FormList", Icon = "fa-solid fa-list", SortNumber = 2, IsVisible = true },
                new UserMenu { Name = "Menü İşlemleri", ParentId = null, ControllerName = "Admin", ActionName = "Index", Icon = "fa-solid fa-cogs", SortNumber = 5, IsVisible = true },
                new UserMenu { Name = "Sertifikalarım", ParentId = null, ControllerName = "Student", ActionName = "Certificate", Icon = "fa-solid fa-certificate", SortNumber = 3, IsVisible = true },
                new UserMenu { Name = "Sınavlarım", ParentId = null, ControllerName = "Student", ActionName = "Exams", Icon = "fa-solid fa-book", SortNumber = 4, IsVisible = true },
                new UserMenu { Name = "Ders İşlemleri", ParentId = null, ControllerName = "Teacher", ActionName = "Classes", Icon = "fa-solid fa-book", SortNumber = 6, IsVisible = true },
                new UserMenu { Name = "Oturum Bilgileri", ParentId = null, ControllerName = "Student", ActionName = "bilgiler", Icon = "fa-solid fa-book", SortNumber = 8, IsVisible = true },
                new UserMenu { Name = "Oturum Bilgileri", ParentId = null, ControllerName = "Admin", ActionName = "Information", Icon = "fa-solid fa-list", SortNumber = 9, IsVisible = true }
            };

            await context.UserMenus.AddRangeAsync(mainMenus);
            await context.SaveChangesAsync();

            // 📌 Ana menülerin ID'lerini veritabanından al
            var menuIslemleri = await context.UserMenus.FirstOrDefaultAsync(m => m.Name == "Menü İşlemleri");
            var dersIslemleri = await context.UserMenus.FirstOrDefaultAsync(m => m.Name == "Ders İşlemleri");

            // 📌 Alt menüleri ekleyelim (ParentId'leri doğru olacak şekilde)
            var subMenus = new List<UserMenu>
            {
                new UserMenu { Name = "Menü Sil", ParentId = menuIslemleri?.Id, ControllerName = "Admin", ActionName = "Index", Icon = "fa-solid fa-trash", SortNumber = 8, IsVisible = true },
                new UserMenu { Name = "Ders Düzenle", ParentId = dersIslemleri?.Id, ControllerName = "Teacher", ActionName = "Classes", Icon = "fa-solid fa-book", SortNumber = 1, IsVisible = true }
            };

            await context.UserMenus.AddRangeAsync(subMenus);
            await context.SaveChangesAsync();
        }
    }
}
