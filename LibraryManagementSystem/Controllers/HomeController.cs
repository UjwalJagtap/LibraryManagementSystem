using Microsoft.AspNetCore.Mvc;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Data;

namespace LibraryManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Role == "Librarian")
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
                else if (model.Role == "Student")
                {
                    return RedirectToAction("Dashboard", "Student");
                }

                ViewBag.ErrorMessage = "Invalid role selected.";
            }
            else
            {
                ViewBag.ErrorMessage = "Please fill in all required fields.";
            }

            return View("Login");
        }
    }
}
