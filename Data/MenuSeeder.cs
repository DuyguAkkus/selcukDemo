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
        if (!await context.UserMenus.AnyAsync())
        {
            var menus = new List<UserMenu>
            {
                new UserMenu { Name = "Anasayfa", ParentId = null, ControllerName = "Home", ActionName = "Dashboard", Icon = "cil-speedometer", SortNumber = 1, IsVisible = true },
                new UserMenu { Name = "Derslerim", ParentId = null, ControllerName = "Student", ActionName = "Courses", Icon = "cil-book", SortNumber = 2, IsVisible = true },
                new UserMenu { Name = "Sertifikalarım", ParentId = null, ControllerName = "Student", ActionName = "Certificate", Icon = "cil-spreadsheet", SortNumber = 3, IsVisible = true },
                new UserMenu { Name = "Sınavlarım", ParentId = null, ControllerName = "Student", ActionName = "Exams", Icon = "cil-task", SortNumber = 4, IsVisible = true },
                new UserMenu { Name = "Oturum Bilgileri", ParentId = null, ControllerName = "Student", ActionName = "bilgiler", Icon = "fa-solid fa-book", SortNumber = 8, IsVisible = true },
                new UserMenu { Name = "Oturum Bilgileri", ParentId = null, ControllerName = "Admin", ActionName = "Information", Icon = "fa-solid fa-list", SortNumber = 9, IsVisible = true },
                new UserMenu { Name = "Ödeme ve Satın Alma", ParentId = null, ControllerName = "User", ActionName = "Payment", Icon = "cil-credit-card", SortNumber = 5, IsVisible = true },
                new UserMenu { Name = "Form Yönetimi", ParentId = null, ControllerName = "User", ActionName = "FormList", Icon = "cil-description", SortNumber = 3, IsVisible = true },
                new UserMenu { Name = "Profil", ParentId = null, ControllerName = "Account", ActionName = "Profile", Icon = "cil-user", SortNumber = 2, IsVisible = true },
                new UserMenu { Name = "Profil Ayarları", ParentId = null, ControllerName = "Account", ActionName = "Settings", Icon = "cil-settings", SortNumber = 2, IsVisible = true },
                new UserMenu { Name = "Eğitim Yönetimi", ParentId = null, ControllerName = "Teacher", ActionName = "MyClasses", Icon = "cil-library", SortNumber = 3, IsVisible = true },
                new UserMenu { Name = "Öğrencilerim", ParentId = null, ControllerName = "Teacher", ActionName = "Students", Icon = "cil-people", SortNumber = 3, IsVisible = true },
                new UserMenu { Name = "Kullanıcı Yönetimi", ParentId = null, ControllerName = "User", ActionName = "UserList", Icon = "cil-people", SortNumber = 6, IsVisible = true },
                new UserMenu { Name = "AnaSayfa", ParentId = null, ControllerName = "Student", ActionName = "HomePages", Icon = "cil-home", SortNumber = 1, IsVisible = true },
                new UserMenu { Name = "AnaSayfa", ParentId = null, ControllerName = "Teacher", ActionName = "HomePageTeacher", Icon = "cil-home", SortNumber = 1, IsVisible = true },
            };

            await context.UserMenus.AddRangeAsync(menus);
            await context.SaveChangesAsync();
        }
    }
}
