using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SelcukDemo.Models;

namespace SelcukDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ILogger<HomeController> logger, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Dashboard", "Home"); // Herkes ortak Dashboard'a gidecek
            }
            return View(); // E�er giri� yap�lmam��sa Welcome sayfas�n� g�ster
        }

        [Authorize] // Kullan�c� giri�i olmadan eri�ilemez!
        public IActionResult Dashboard()
        {
            return View(); // Herkes ayn� Dashboard sayfas�na gidiyor ama men� rollere g�re de�i�iyor!
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpGet]
        public IActionResult GetUserMenus()
        {
            var menuClaims = User.Claims
                .Where(c => c.Type == "Menu")
                .Select(c => c.Value)
                .ToList();

            _logger.LogInformation("Kullanıcı Menüsü: {@MenuClaims}", menuClaims);

            return Json(menuClaims);
        }

    }
}