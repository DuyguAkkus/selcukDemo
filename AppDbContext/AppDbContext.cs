using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SelcukDemo.Models;

namespace SelcukDemo.AppDbContext
{
    public class SelcukDbContext : IdentityDbContext<AppUser>
    {
        public SelcukDbContext(DbContextOptions<SelcukDbContext> options) : base(options) {}

        public DbSet<UserMenu> UserMenus { get; set; }
        public DbSet<RoleMenu> RoleMenus { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<UploadedFile> UploadedFiles { get; set; }
    }

}