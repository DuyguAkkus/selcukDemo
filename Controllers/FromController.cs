using Microsoft.AspNetCore.Mvc;
using SelcukDemo.AppDbContext;
using SelcukDemo.Models;
using SelcukDemo.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SelcukDemo.Controllers
{
    public class FormController : Controller
    {
        private readonly SelcukDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public FormController(SelcukDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // 1. Formları listele (kategori filtresiyle birlikte)
        public async Task<IActionResult> Index(string? category)
        {
            var allCategories = new List<string> { "Rapor", "Duyuru", "Belge" };
            ViewBag.Categories = new SelectList(allCategories);
            ViewBag.SelectedCategory = category;

            var query = _context.UploadedFiles.AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(f => f.Category == category);
            }

            var forms = await query.ToListAsync();
            return View(forms);
        }

        // 2. Form ekle - GET
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UploadedFileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Dosya yükleme yolu
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/forms");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var fileName = Guid.NewGuid() + Path.GetExtension(model.File.FileName);
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.File.CopyToAsync(stream);
            }

            // Veritabanı için asıl model
            var uploaded = new UploadedFile
            {
                FileName = model.FileName,
                Description = model.Description,
                FilePath = "images/forms/" + fileName,
                Category = model.Category,
                UploadedAt = DateTime.Now
            };

            _context.UploadedFiles.Add(uploaded);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var form = await _context.UploadedFiles.FindAsync(id);
            if (form == null) return NotFound();

            return View(form);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, UploadedFile model, IFormFile? UploadedFile)
        {
            var form = await _context.UploadedFiles.FindAsync(id);
            if (form == null) return NotFound();

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"{error.Key} => {string.Join(",", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
                return View(model);
            }

            // Eğer yeni dosya yüklendiyse dosyayı değiştir
            if (UploadedFile != null)
            {
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/forms");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var newFileName = Guid.NewGuid() + Path.GetExtension(UploadedFile.FileName);
                var filePath = Path.Combine(uploadPath, newFileName);

                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await UploadedFile.CopyToAsync(stream);
                }

                form.FilePath = "images/forms/" + newFileName;
            }

            // Diğer alanları güncelle
            form.FileName = model.FileName;
            form.Description = model.Description;
            form.Category = model.Category;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // 6. Form sil
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var form = await _context.UploadedFiles.FindAsync(id);
            if (form == null)
            {
                return NotFound();
            }

            // Dosya fiziksel olarak da varsa sil
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", form.FilePath);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            _context.UploadedFiles.Remove(form);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
