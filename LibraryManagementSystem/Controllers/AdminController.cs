using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using LibraryManagementSystem.Data;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

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
                ViewBag.HeaderController = "Admin";
                ViewBag.HeaderAction = "Dashboard";

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
                return PartialView("Books", books);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading books: {ex.Message}";
                return RedirectToAction("Dashboard");
            }
        }
        [HttpPost]
        public IActionResult Books(string searchQuery)
        {
            var books = string.IsNullOrWhiteSpace(searchQuery)
                ? _context.Books.ToList() // Return all books if no query
                : _context.Books.Where(b => b.Title.Contains(searchQuery) ||
                                            b.Author.Contains(searchQuery) ||
                                            b.Genre.Contains(searchQuery)).ToList(); // Filter by title, author, or genre

            return PartialView("Books", books); // Return updated list in the same partial view
        }
        public IActionResult AddBooks()
        {
            return PartialView("AddBooks");
        }

        [HttpPost]
        public IActionResult AddBook(Book model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("AddBooks", model);
            }

            try
            {
                if (model.AvailableCopies > model.TotalCopies)
                {
                    return Json(new { success = false, message = "Available copies cannot exceed total copies." });
                }
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

            return PartialView("UpdateBook", book);
        }

        [HttpPost]
        public IActionResult UpdateBook(Book model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("UpdateBook", model);
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
            return PartialView("StudentInfo", students);
        }

        [HttpPost]
        public IActionResult SearchStudents(string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                return Json(new { success = false, message = "Please enter a search query." });
            }

            // Search students based on name, email, or phone
            var students = _context.Users
                .Where(u => u.Role == "Student" &&
                            (u.FullName.Contains(searchQuery) ||
                             u.Email.Contains(searchQuery) ||
                             u.Phone.Contains(searchQuery)))
                .ToList();

            return Json(new { success = true, students });
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
                var metrics = GetMetrics();
                return Json(new { success = true, message = "Student removed successfully!", students,metrics });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error deleting student: {ex.Message}" });
            }
        }

        // Load all book requests
        public IActionResult ManageBookRequests()
        {
            var requests = _context.BookRequests
                .Include(r => r.Book)  // Eager loading to include Book details
                .Include(r => r.User)  // Eager loading to include User details
                .Select(r => new BookRequest
                {
                    RequestId = r.RequestId,
                    Book = r.Book,
                    User = r.User,
                    RequestDate = r.RequestDate,
                    Status = r.Status
                })
                .ToList();

            return PartialView("ManageBookRequests", requests);
        }


        // Approve book request
        [HttpPost]
        public IActionResult ApproveRequest(int requestId)
        {
            var request = _context.BookRequests.FirstOrDefault(r => r.RequestId == requestId);
            if (request == null)
                return Json(new { success = false, message = "Request not found." });

            var book = _context.Books.FirstOrDefault(b => b.BookId == request.BookId);
            if (book == null)
                return Json(new { success = false, message = "Book not found." });

            // Check if available copies exist before approval
            if (book.AvailableCopies <= 0)
            {
                request.Status = "Rejected";
                _context.SaveChanges();
                return Json(new { success = false, message = "Book is unavailable and the request is rejected." });
            }

            request.Status = "Approved";

            // Decrease available copies here
            book.AvailableCopies -= 1;

            // Issue the book to the student
            var issuedBook = new IssuedBook
            {
                BookId = request.BookId,
                UserId = request.UserId,
                IssueDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(14) // 2 weeks borrowing time
            };
            _context.IssuedBooks.Add(issuedBook);

            _context.SaveChanges();

            // Fetch updated metrics
            var metrics = GetMetrics();

            return Json(new { success = true, message = "Request approved successfully!", metrics });
        }

        [HttpPost]
        public IActionResult RejectRequest(int requestId)
        {
            var request = _context.BookRequests.FirstOrDefault(r => r.RequestId == requestId);
            if (request == null)
                return Json(new { success = false, message = "Request not found." });

            request.Status = "Rejected";

            _context.SaveChanges();

            return Json(new { success = true, message = "Request rejected successfully!" });
        }
        [HttpGet]
        public IActionResult FilterBookRequests(string status)
        {
            IQueryable<BookRequest> requestsQuery = _context.BookRequests
                .Include(r => r.Book)  // Eager load Book details
                .Include(r => r.User); // Eager load User details

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                // Apply the status filter if it's not "All"
                requestsQuery = requestsQuery.Where(r => r.Status == status);
            }

            var filteredRequests = requestsQuery.ToList(); // Convert query to list
            return PartialView("ManageBookRequests", filteredRequests); // Return the same view with filtered data
        }



        public IActionResult ManageFines()
        {
            return PartialView("ManageFines");
        }

        public IActionResult GenerateReports()
        {
            return PartialView("GenerateReports");
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
