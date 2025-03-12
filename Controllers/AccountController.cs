using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SelcukDemo.Services;
using SelcukDemo.ViewModels;

namespace SelcukDemo.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ClaimsService _claimsService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<IdentityUser> userManager, 
                                 SignInManager<IdentityUser> signInManager, 
                                 RoleManager<IdentityRole> roleManager, 
                                 ClaimsService claimsService, 
                                 ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _claimsService = claimsService;
            _logger = logger;
        }

        // 📌 Giriş Sayfası (GET)
        [HttpGet]
        public IActionResult SignIn()
        {
            return View();
        }

        // 📌 Kullanıcının Yetkilerini JSON Olarak Döndür (Debug için)
        [HttpGet]
        public IActionResult GetUserClaims()
        {
            var userClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            return Json(userClaims);
        }

        // 📌 Kullanıcının Rolünü JSON Olarak Döndür
        [HttpGet]
        public async Task<IActionResult> GetUserRole()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                return Json(new { role = roles.Count > 0 ? roles[0] : "None" });
            }
            return Json(new { role = "Guest" });
        }

        // 📌 Kullanıcı Girişi (POST)
        [HttpPost]
        public async Task<IActionResult> SignIn(SignInViewModel_ model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Giriş başarısız: Model geçersiz.");
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                _logger.LogWarning("Giriş başarısız: Kullanıcı bulunamadı ({Email})", model.Email);
                ModelState.AddModelError(string.Empty, "Email veya şifre yanlış.");
                return View(model);
            }

            // 🔥 Önce eski oturumu temizleyelim
            await _signInManager.SignOutAsync();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, true);
            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("Kullanıcı kilitlendi: {Email}", model.Email);
                    ModelState.AddModelError(string.Empty, "Çok fazla başarısız giriş yaptınız, bir süre bekleyin.");
                }
                else
                {
                    _logger.LogWarning("Giriş başarısız: Şifre yanlış ({Email})", model.Email);
                    ModelState.AddModelError(string.Empty, "Email veya şifre yanlış.");
                }
                return View(model);
            }

            // 📌 Kullanıcının Yetkilerini ve Rolleri Yükle
            var claims = await _claimsService.GetUserClaims(user);
            var userRoles = await _userManager.GetRolesAsync(user);

            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));  
            }

            if (claims.Any())
            {
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                // 🔥 Yeni claim'lerle oturumu başlat
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    claimsPrincipal,
                    new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = DateTime.UtcNow.AddHours(1)
                    });

                _logger.LogInformation("Giriş başarılı: {Email}", user.Email);
            }
            else
            {
                _logger.LogError("Kullanıcı için herhangi bir menü veya rol claimi bulunamadı: {Email}", user.Email);
            }

            return RedirectToAction("Index", "Home"); // 📌 Giriş başarılıysa Anasayfa'ya yönlendir
        }

        // 📌 Kullanıcı Çıkışı (Logout)
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync(); 
            await HttpContext.SignOutAsync(); 
            return RedirectToAction("SignIn", "Account");
        }

        // 📌 Kullanıcı Kayıt Sayfası (GET)
        public IActionResult SignUp()
        {
            return View();
        }

        // 📌 Öğrenci Kaydı (POST)
        [HttpPost]
        public async Task<IActionResult> RegisterStudent(SignUpViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Form geçerli değil!");
                return View("SignUp", model);
            }

            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = model.Phone
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync("Student"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Student"));
                }

                await _userManager.AddToRoleAsync(user, "Student");
                TempData["SuccessMessage"] = "Kayıt başarılı!";
                return RedirectToAction("SignIn", "Account");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View("SignUp", model);
        }

        // 📌 Öğretmen Kaydı (POST)
        [HttpPost]
        public async Task<IActionResult> RegisterTeacher(SignUpViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Form geçerli değil!");
                return View("SignUp", model);
            }

            if (!model.Email.EndsWith("@edu.tr"))
            {
                ModelState.AddModelError("Email", "Öğretmen kaydı için e-posta @edu.tr uzantılı olmalıdır.");
                return View("SignUp", model);
            }

            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = model.Phone
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync("Teacher"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Teacher"));
                }

                await _userManager.AddToRoleAsync(user, "Teacher");
                TempData["SuccessMessage"] = "Kayıt başarılı!";
                return RedirectToAction("SignIn", "Account");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View("SignUp", model);
        }
    }
}
