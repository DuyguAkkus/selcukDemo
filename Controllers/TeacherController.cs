using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SelcukDemo.Models;

namespace SelcukDemo.Controllers
{
    [CustomAuthorize("Teacher","Admin")]
    public class TeacherController : Controller
    {
        private readonly AppDbContext.SelcukDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TeacherController(AppDbContext.SelcukDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // **📌 1. Dersleri Listeleme**
        public async Task<IActionResult> Classes()
        {
            var user = await _userManager.GetUserAsync(User);
            var lessons = await _context.Lessons
                .Where(l => l.TeacherId == user.Id || User.IsInRole("Admin")) // Öğretmen sadece kendi eklediklerini görsün
                .ToListAsync();

            return View(lessons);
        }

        // **📌 2. Yeni Ders Ekleme (GET)**
        [HttpGet]
        public IActionResult AddClass()
        {
            return View();
        }

        // **📌 3. Yeni Ders Ekleme (POST)**
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddClass(Lesson lesson)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                lesson.TeacherId = user.Id;

                _context.Lessons.Add(lesson);
                await _context.SaveChangesAsync();

                return RedirectToAction("Classes");
            }
            return View(lesson);
        }

        // **📌 4. Ders Silme**
        [HttpPost]
        public async Task<IActionResult> DeleteClass(int id)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null) return NotFound();

            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();

            return RedirectToAction("Classes");
        }

        // **📌 5. Öğrencileri Listeleme**
        public IActionResult Students()
        {
            return View(); // Burada ilerleyen süreçte öğrenci işlemleri olacak
        }
       
      


    }
}