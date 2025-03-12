using Microsoft.AspNetCore.Mvc;

namespace SelcukDemo.Controllers
{
    [CustomAuthorize("Student")]
    public class StudentController : Controller
    {
        
        public IActionResult Courses()
        {
            return View();
        }
        public IActionResult Certificate()
        {
            return View();
        }
        public IActionResult Exams()
        {
            return View();
        }
    }
}