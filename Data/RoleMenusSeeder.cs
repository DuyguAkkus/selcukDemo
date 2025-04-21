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
            if (!await context.RoleMenus.AnyAsync())
            {
                var roleMenus = new List<RoleMenu>
                {
                    // Admin Menüler
                    new RoleMenu { RoleName = "Admin", MenuId = 8 },   // Form Yönetimi
                    new RoleMenu { RoleName = "Admin", MenuId = 13 },  // Kullanıcı Yönetimi
                    new RoleMenu { RoleName = "Admin", MenuId = 9 },   // Profil
                    new RoleMenu { RoleName = "Admin", MenuId = 10 },  // Profil Ayarları
                    new RoleMenu { RoleName = "Admin", MenuId = 6 },   // Oturum Bilgileri (Admin)
                    new RoleMenu { RoleName = "Admin", MenuId = 1 },   // Anasayfa

                    // Teacher Menüler
                    new RoleMenu { RoleName = "Teacher", MenuId = 11 }, // Eğitim Yönetimi
                    new RoleMenu { RoleName = "Teacher", MenuId = 12 }, // Öğrencilerim
                    new RoleMenu { RoleName = "Teacher", MenuId = 9 },  // Profil
                    new RoleMenu { RoleName = "Teacher", MenuId = 10 }, // Profil Ayarları
                    new RoleMenu { RoleName = "Teacher", MenuId = 15 }, // AnaSayfa (Teacher)

                    // Student Menüler
                    new RoleMenu { RoleName = "Student", MenuId = 10 }, // Profil Ayarları
                    new RoleMenu { RoleName = "Student", MenuId = 2 },  // Derslerim
                    new RoleMenu { RoleName = "Student", MenuId = 3 },  // Sertifikalarım
                    new RoleMenu { RoleName = "Student", MenuId = 4 },  // Sınavlarım
                    new RoleMenu { RoleName = "Student", MenuId = 5 },  // Oturum Bilgileri (Student)
                    new RoleMenu { RoleName = "Student", MenuId = 14 }, // AnaSayfa (Student)
                };

                await context.RoleMenus.AddRangeAsync(roleMenus);
                await context.SaveChangesAsync();
            }
        }
    }

