using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using LibraryManagementSystem.Data;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;

namespace LibraryManagementSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly LibraryContext _context;

        public AdminController(LibraryContext context)
        {
            _context = context;
        }

        // Ensure only logged-in Admins can access this controller
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!UserIsAdmin())
            {
                TempData["ErrorMessage"] = "Access denied. Admins only.";
                context.Result = RedirectToAction("Index", "Home");
            }
            base.OnActionExecuting(context);
        }

        private bool UserIsAdmin()
        {
            // Use session to validate if the user is an admin
            var role = HttpContext.Session.GetString("Role"); // Ensure the "Role" is set during login
            return role == "Librarian"; // Only "Librarian" is allowed to access the Admin panel
        }
         

        public IActionResult Dashboard()
        {
            // Make sure to check if the data exists before passing to the view
            try
            {
                // Pass metrics to the view
                ViewBag.TotalBooks = _context.Books.Count();
                ViewBag.TotalStudents = _context.Users.Count(u => u.Role == "Student");
                ViewBag.TotalIssuedBooks = _context.IssuedBooks.Count();
                ViewBag.TotalOverdueBooks = _context.IssuedBooks.Count(ib => ib.DueDate < DateTime.Now && ib.ReturnDate == null);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading dashboard: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        public IActionResult AddBooks()
        {
            // Returning a partial view to handle book addition logic
            return PartialView("_AddBooks");
        }

        public IActionResult IssueBooks()
        {
            // Returning a partial view to handle book issuance logic
            return PartialView("_IssueBooks");
        }

        public IActionResult ManageFines()
        {
            // Returning a partial view to handle fines management logic
            return PartialView("_ManageFines");
        }

        public IActionResult GenerateReports()
        {
            // Returning a partial view to handle reports generation logic
            return PartialView("_GenerateReports");
        }
    }
}
