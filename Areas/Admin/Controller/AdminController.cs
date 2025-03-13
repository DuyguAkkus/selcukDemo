using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SelcukDemo.AppDbContext;
using SelcukDemo.Areas.Admin.Models;
using SelcukDemo.Models;
using SelcukDemo.Services;

namespace SelcukDemo.Areas.Admin.Controller // ✅ DOĞRU
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SelcukDbContext _context;
        private readonly SignInManager<IdentityUser> _signInManager;
        private object _contextFactory;
        private object _logger;

        public AdminController(UserManager<IdentityUser> userManager, SelcukDbContext context,
            SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _context = context;
            _signInManager = signInManager;

        }

        public async Task<IActionResult> Index()
        {
            var menus = await _context.UserMenus.OrderBy(m => m.SortNumber).ToListAsync();
            return View(menus);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Menus = _context.UserMenus.Where(m => m.ParentId == null).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserMenu menu)
        {
            if (!ModelState.IsValid)
            {
                int newSortNumber = menu.ParentId == null
                    ? (_context.UserMenus.Where(m => m.ParentId == null).Max(m => (int?)m.SortNumber) ?? 0) + 1
                    : (_context.UserMenus.Where(m => m.ParentId == menu.ParentId).Max(m => (int?)m.SortNumber) ?? 0) + 1;

                menu.SortNumber = newSortNumber;

                _context.UserMenus.Add(menu);
                await _context.SaveChangesAsync();

                if (!string.IsNullOrEmpty(menu.SelectedRole))
                {
                    var roleMenu = new RoleMenu { RoleName = menu.SelectedRole, MenuId = menu.Id };
                    _context.RoleMenus.Add(roleMenu);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("Index");
            }

            return View(menu);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var menu = await _context.UserMenus.FindAsync(id);
            if (menu == null) return NotFound();

            ViewBag.Roles = await _context.Set<IdentityRole>().Select(r => r.Name).ToListAsync();
            ViewBag.Menus = await _context.UserMenus.Where(m => m.ParentId == null).ToListAsync();
            return View(menu);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserMenu menu)
        {
            if (id != menu.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                try
                {
                    var existingMenu = await _context.UserMenus.FindAsync(id);
                    if (existingMenu != null)
                    {
                        existingMenu.Name = menu.Name;
                        existingMenu.ControllerName = menu.ControllerName;
                        existingMenu.ActionName = menu.ActionName;
                        existingMenu.SortNumber = menu.SortNumber;
                        existingMenu.ParentId = menu.ParentId;
                        existingMenu.IsVisible = true;

                        _context.Update(existingMenu);
                        await _context.SaveChangesAsync();

                        var user = await _userManager.GetUserAsync(User);
                        if (user != null)
                        {
                            var claimsService = new ClaimsService(_userManager, _contextFactory, _logger);
                            await claimsService.UpdateUserClaims(user);
                            await _signInManager.RefreshSignInAsync(user);
                        }
                    }

                    return RedirectToAction("Index", new { area = "Admin" });
                }
                catch (Exception ex)
                {
                  
                    ModelState.AddModelError("", "Güncelleme hatası: " + ex.Message);
                }
            }

            ViewBag.Roles = await _context.Set<IdentityRole>().Select(r => r.Name).ToListAsync();
            ViewBag.Menus = _context.UserMenus.Where(m => m.ParentId == null).ToList();

            return View(menu);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var menu = await _context.UserMenus.FindAsync(id);
            if (menu == null) return NotFound();

            _context.UserMenus.Remove(menu);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> UserList()
        {
            var users = _userManager.Users.ToList();
            var userRoles = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles.Add(new UserRoleViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = roles.ToList()
                });
            }

            return View(userRoles);
        }
    } // ✅ FAZLADAN `;` KALDIRILDI
}
