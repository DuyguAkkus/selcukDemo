using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SelcukDemo.Models;
using SelcukDemo.Services;
using SelcukDemo.ViewModels;

namespace SelcukDemo.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ClaimsService _claimsService;
        private readonly ILogger<AccountController> _logger;
        private readonly IWebHostEnvironment _env;

        public AccountController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ClaimsService claimsService,
            ILogger<AccountController> logger,
            IWebHostEnvironment env)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _claimsService = claimsService;
            _logger = logger;
            _env = env;
        }

        [HttpGet]
        public IActionResult SignIn() => View();

        [HttpGet]
        public IActionResult GetUserClaims()
        {
            var userClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            return Json(userClaims);
        }

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

            await _signInManager.SignOutAsync();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, true);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Giriş başarısız: {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "Email veya şifre yanlış.");
                return View(model);
            }

            await _claimsService.UpdateUserClaims(user);
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

            if (await _userManager.IsInRoleAsync(user, "Admin"))
                return RedirectToAction("Dashboard", "Home");
            else if (await _userManager.IsInRoleAsync(user, "Student"))
                return RedirectToAction("HomePages", "Student");
            else if (await _userManager.IsInRoleAsync(user, "Teacher"))
                return RedirectToAction("HomePageTeachers", "Teacher");
            else
                return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            await HttpContext.SignOutAsync();
            return RedirectToAction("SignIn", "Account");
        }

        public IActionResult SignUp() => View();

        [HttpPost]
        public async Task<IActionResult> RegisterStudent(SignUpViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Form geçerli değil!");
                return View("SignUp", model);
            }

            var user = new AppUser
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

            var user = new AppUser
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

        [HttpGet]
        public IActionResult DebugClaims()
        {
            var userClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();

            foreach (var claim in User.Claims)
            {
                Console.WriteLine($"DebugClaims -> Type: {claim.Type}, Value: {claim.Value}");
            }

            return Json(userClaims);
        }

        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("SignIn");

            var model = new ProfileSettingsViewModel
            {
                BirthDate = user.BirthDate,
                Gender = user.Gender,
                City = user.City
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Settings(ProfileSettingsViewModel model)
        {
            
            if (!ModelState.IsValid)
            {
                TempData["ShowRequiredMessage"] = "true"; // tekrar zorunlu mesajı gösterilsin
                return View(model);
            }

            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("SignIn");

            user.BirthDate = model.BirthDate;
            user.Gender = model.Gender;
            user.City = model.City;

            // Görsel yüklendiyse kaydet
            if (model.ProfileImage != null)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(model.ProfileImage.FileName);
                var savePath = Path.Combine(_env.WebRootPath, "images/profiles", fileName);

                Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);

                using var stream = new FileStream(savePath, FileMode.Create);
                await model.ProfileImage.CopyToAsync(stream);

                user.ProfileImagePath = "/images/profiles/" + fileName;
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "Profiliniz başarıyla güncellendi.";
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound("Kullanıcı bulunamadı.");

            var model = new UserProfileViewModel
            {
                UserName = user.UserName,
                Email = user.Email,
                BirthDate = user.BirthDate,
                Gender = user.Gender,
                City = user.City,
                ProfileImagePath = user.ProfileImagePath
            };

            return View(model);
        }

    }
}
