using Microsoft.AspNetCore.Mvc;
using SelcukDemo.Filters;

namespace SelcukDemo.Controllers
{
    [RequireCompleteProfile]
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
        public IActionResult HomePages()
        {
            return View();
        }
    }
}