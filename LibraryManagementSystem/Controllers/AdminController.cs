using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using LibraryManagementSystem.Data;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System;
using Microsoft.AspNetCore.Mvc.Filters;

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
            var role = HttpContext.Session.GetString("Role");
            return role == "Librarian";
        }

        public IActionResult Dashboard()
        {
            try
            {
                UpdateMetrics();
                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading dashboard: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }

        public IActionResult Books()
        {
            try
            {
                var books = _context.Books.ToList();
                return PartialView("_Books", books);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading books: {ex.Message}";
                return RedirectToAction("Dashboard");
            }
        }

        public IActionResult AddBooks()
        {
            return PartialView("_AddBooks");
        }

        [HttpPost]
        public IActionResult AddBook(Book model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_AddBooks", model);
            }

            try
            {
                _context.Books.Add(model);
                _context.SaveChanges();

                // Fetch updated data for metrics and books
                var books = _context.Books.ToList();
                var metrics = GetMetrics();

                return Json(new
                {
                    success = true,
                    message = "Book added successfully!",
                    books,
                    metrics
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error adding book: {ex.Message}" });
            }
        }

        [HttpGet]
        public IActionResult UpdateBook(int id)
        {
            var book = _context.Books.Find(id);
            if (book == null)
                return NotFound();

            return PartialView("_UpdateBook", book);
        }

        [HttpPost]
        public IActionResult UpdateBook(Book model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_UpdateBook", model);
            }

            try
            {
                var book = _context.Books.Find(model.BookId);
                if (book == null)
                    return Json(new { success = false, message = "Book not found." });

                if (model.AvailableCopies > model.TotalCopies)
                {
                    return Json(new { success = false, message = "Available copies cannot exceed total copies." });
                }

                // Update book details
                book.Title = model.Title;
                book.Author = model.Author;
                book.Publisher = model.Publisher;
                book.ISBN = model.ISBN;
                book.PublishedYear = model.PublishedYear;
                book.Genre = model.Genre;
                book.TotalCopies = model.TotalCopies;
                book.AvailableCopies = model.AvailableCopies;

                _context.SaveChanges();

                // Fetch updated data for metrics and books
                var books = _context.Books.ToList();
                var metrics = GetMetrics();

                return Json(new
                {
                    success = true,
                    message = "Book updated successfully!",
                    books,
                    metrics
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error updating book: {ex.Message}" });
            }
        }

        [HttpPost]
        public IActionResult DeleteBook(int id)
        {
            try
            {
                var book = _context.Books.Find(id);
                if (book == null)
                    return Json(new { success = false, message = "Book not found." });

                _context.Books.Remove(book);
                _context.SaveChanges();

                // Fetch updated data for metrics and books
                var books = _context.Books.ToList();
                var metrics = GetMetrics();

                return Json(new
                {
                    success = true,
                    message = "Book deleted successfully!",
                    books,
                    metrics
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error deleting book: {ex.Message}" });
            }
        }
        public IActionResult StudentInfo()
        {
            var students = _context.Users.Where(u => u.Role == "Student").ToList();
            return PartialView("_StudentInfo", students);
        }
        [HttpPost]
        public IActionResult DeleteStudent(int id)
        {
            try
            {
                var student = _context.Users.FirstOrDefault(u => u.UserId == id && u.Role == "Student");
                if (student == null)
                {
                    return Json(new { success = false, message = "Student not found." });
                }

                _context.Users.Remove(student);
                _context.SaveChanges();

                var students = _context.Users.Where(u => u.Role == "Student").ToList();
                return Json(new { success = true, message = "Student removed successfully!", students });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error deleting student: {ex.Message}" });
            }
        }


        public IActionResult IssueBooks()
        {
            return PartialView("_IssueBooks");
        }

        public IActionResult ManageFines()
        {
            return PartialView("_ManageFines");
        }

        public IActionResult GenerateReports()
        {
            return PartialView("_GenerateReports");
        }

        private void UpdateMetrics()
        {
            ViewBag.TotalBooks = _context.Books.Count();
            ViewBag.TotalStudents = _context.Users.Count(u => u.Role == "Student");
            ViewBag.TotalIssuedBooks = _context.IssuedBooks.Count();
            ViewBag.TotalOverdueBooks = _context.IssuedBooks.Count(ib => ib.DueDate < DateTime.Now && ib.ReturnDate == null);
        }

        private object GetMetrics()
        {
            return new
            {
                TotalBooks = _context.Books.Count(),
                TotalStudents = _context.Users.Count(u => u.Role == "Student"),
                TotalIssuedBooks = _context.IssuedBooks.Count(),
                TotalOverdueBooks = _context.IssuedBooks.Count(ib => ib.DueDate < DateTime.Now && ib.ReturnDate == null)
            };
        }
    }
}
