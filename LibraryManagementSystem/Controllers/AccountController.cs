using Microsoft.AspNetCore.Mvc;
using LibraryManagementSystem.Models; // Update with the correct namespace for your models

namespace LibraryManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly LibraryContext _context;

        public AccountController(LibraryContext context)
        {
            _context = context;
        }

        // GET: Registration Page
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Handle Registration Form Submission
        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    PasswordHash = model.Password, // Encrypt the password in production
                    Role = "Student"
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Registration successful! Please log in.";
                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }
    }
}
