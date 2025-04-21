using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SelcukDemo.AppDbContext;
using SelcukDemo.Areas.Admin.Models;
using SelcukDemo.Models;
using SelcukDemo.Services;

namespace SelcukDemo.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SelcukDbContext _context;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ClaimsService _claimsService;

        public AdminController(
            UserManager<AppUser> userManager,
            SelcukDbContext context,
            SignInManager<AppUser> signInManager,
            ClaimsService claimsService)
        {
            _userManager = userManager;
            _context = context;
            _signInManager = signInManager;
            _claimsService = claimsService;
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
                    Roles = roles.ToList(),
                    ProfileImagePath = user.ProfileImagePath,
                    CreatedAt = user.CreatedAt
                });
            }

            return View(userRoles);
            
        }

    }
}
