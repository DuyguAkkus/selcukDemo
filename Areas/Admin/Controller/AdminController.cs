using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SelcukDemo.Models;
using SelcukDemo.Services;

namespace SelcukDemo.Areas.Admin.Controller
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppDbContext.SelcukDbContext _context;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ClaimsService _claimsService;

        public AdminController(UserManager<IdentityUser> userManager, AppDbContext.SelcukDbContext context,
            SignInManager<IdentityUser> signInManager, ClaimsService claimsService)
        {
            _userManager = userManager;
            _context = context;
            _signInManager = signInManager;
            _claimsService = claimsService;
        }

        public async Task<IActionResult> Edit(int id, UserMenu menu)
        {
            if (id != menu.Id)
                return NotFound();

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
                            await _claimsService.UpdateUserClaims(user);
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

            ViewBag.Roles = await _context.Set<IdentityRole>().Select(r => r.Name).ToListAsync() ?? new List<string>();
            ViewBag.Menus = _context.UserMenus.Where(m => m.ParentId == null).ToList();

            return View(menu);
        }
    }
}
