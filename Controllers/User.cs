using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SelcukDemo.Models; // Not: ViewModel'in artık Admin.Area içinde değilse, taşı ya da namespace'i değiştir
using System.Threading.Tasks;
using SelcukDemo.Areas.Admin.Models;

namespace SelcukDemo.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public UserController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> UserList()
        {
            var users = _userManager.Users.ToList();
            var userRoles = new List<UserListViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles.Add(new UserListViewModel
                {
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = roles.ToList()
                });
            }

            return View(userRoles);
        }
    }
}