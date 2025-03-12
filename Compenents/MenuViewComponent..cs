using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using SelcukDemo.AppDbContext;
using SelcukDemo.Models;

namespace MenuProject.Components
{
    public class MenuViewComponent : ViewComponent
    {
        private readonly SelcukDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MenuViewComponent(SelcukDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = _httpContextAccessor.HttpContext.User;

            // Kullanıcının sahip olduğu yetkileri claim'lerden çek
            var menuClaims = user.Claims
                                .Where(c => c.Type == "Menu")
                                .Select(c => c.Value)
                                .ToList();

            // Veritabanından menüleri çek
            var allMenus = _context.UserMenus
                                .Where(m => m.IsVisible)
                                .OrderBy(m => m.SortNumber)
                                .ToList();

            // Ana menüleri ve alt menüleri belirle
            var mainMenus = allMenus
                            .Where(m => m.ParentId == null &&
                                        menuClaims.Contains($"{m.ControllerName}/{m.ActionName}/{m.Name}"))
                            .ToList();

            var subMenus = allMenus
                            .Where(m => m.ParentId != null &&
                                        menuClaims.Contains($"{m.ControllerName}/{m.ActionName}/{m.Name}"))
                            .ToList();

            var model = new MenuViewModel
            {
                MainMenus = mainMenus,
                SubMenus = subMenus
            };

            return View(model);
        }
    }

    public class MenuViewModel
    {
        public List<UserMenu> MainMenus { get; set; }
        public List<UserMenu> SubMenus { get; set; }
    }
}