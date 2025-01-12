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
             .Where(r => r.Status != "Returned")  // Filter out requests with status "Returned"
             .Select(r => new BookRequest
             {
                 RequestId = r.RequestId,
                 Book = r.Book,
                 User = r.User,
                 RequestType = r.RequestType,
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
            var request = _context.BookRequests.Include(r => r.Book).Include(r => r.User).FirstOrDefault(r => r.RequestId == requestId);
            if (request == null)
                return Json(new { success = false, message = "Request not found." });

            var book = request.Book;
            if (book == null)
                return Json(new { success = false, message = "Book not found." });

            // Check for available copies only for new requests
            if (request.RequestType == "New" && book.AvailableCopies <= 0)
            {
                request.Status = "Rejected";
                _context.SaveChanges();
                return Json(new { success = false, message = "Book is unavailable and the request is rejected." });
            }

            request.Status = "Approved";

            // Handle the different types of requests
            if (request.RequestType == "New")
            {
                book.AvailableCopies -= 1; // Decrease available copies for new request
                _context.IssuedBooks.Add(new IssuedBook
                {
                    BookId = request.BookId,
                    UserId = request.UserId,
                    IssueDate = DateTime.Now,
                    DueDate = DateTime.Now.AddDays(14) // Default borrowing period
                });
            }
            else if (request.RequestType == "Renewal")
            {
                var issuedBook = _context.IssuedBooks.FirstOrDefault(ib => ib.BookId == request.BookId && ib.UserId == request.UserId && ib.ReturnDate == null);
                if (issuedBook != null)
                {
                    issuedBook.DueDate = issuedBook.DueDate.AddDays(14); // Extend due date by 14 days
                }
            }

            _context.SaveChanges();
            var metrics = GetMetrics();
            return Json(new { success = true, message = "Request approved successfully!", metrics });
        }

        [HttpPost]
        public IActionResult RenewBook(int issuedBookId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return Json(new { success = false, message = "User not logged in." });

            var issuedBook = _context.IssuedBooks.FirstOrDefault(ib => ib.IssuedBookId == issuedBookId && ib.UserId == userId.Value && ib.ReturnDate == null);
            if (issuedBook == null)
                return Json(new { success = false, message = "Issued book not found." });

            // Add renewal request to BookRequests table
            var renewalRequest = new BookRequest
            {
                UserId = userId.Value,
                BookId = issuedBook.BookId,
                RequestDate = DateTime.Now,
                Status = "Pending",
                RequestType = "Renewal" // Set the request type as "Renewal"
            };

            _context.BookRequests.Add(renewalRequest);
            _context.SaveChanges();

            return Json(new { success = true, message = "Renewal request has been successfully submitted!" });
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
            try
            {
                IQueryable<BookRequest> requestsQuery = _context.BookRequests
                    .Include(r => r.Book)  // Eager load Book details
                    .Include(r => r.User)
                    .Where(r => r.Status != "Returned"); 

                if (!string.IsNullOrWhiteSpace(status) && status != "All")
                {
                    // Apply the status filter if it's not "All"
                    requestsQuery = requestsQuery.Where(r => r.Status == status);
                }

                var filteredRequests = requestsQuery.ToList(); // Convert query to list
                return PartialView("ManageBookRequests", filteredRequests); // Return the same view with filtered data
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error loading filtered requests: {ex.Message}" });
            }
        }
        public IActionResult ViewIssuedBooks()
        {
            try
            {
                var issuedBooks = _context.IssuedBooks
                    .Include(ib => ib.Book) // Include Book details
                    .Include(ib => ib.User) // Include Student details
                    .Select(ib => new
                    {
                        ib.BookId,
                        BookTitle = ib.Book.Title,
                        StudentName = ib.User.FullName,
                        ib.IssueDate,
                        ib.DueDate,
                        ib.ReturnDate,
                        Status = ib.ReturnDate != null ? "Returned" : (ib.DueDate < DateTime.Now ? "Overdue" : "On Time")
                    }).ToList();

                return PartialView("ViewIssuedBooks", issuedBooks);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading issued books: {ex.Message}";
                return Json(new { success = false, message = $"Error loading issued books view: {ex.Message}" });
            }
        }
        [HttpGet]
        public IActionResult SearchIssuedBooks(string searchQuery)
        {
            var issuedBooksQuery = _context.IssuedBooks
                .Include(ib => ib.Book)
                .Include(ib => ib.User)
                .Select(ib => new
                {
                    ib.BookId,
                    BookTitle = ib.Book.Title,
                    StudentName = ib.User.FullName,
                    ib.IssueDate,
                    ib.DueDate,
                    ib.ReturnDate,
                    Status = ib.ReturnDate != null ? "Returned" : (ib.DueDate < DateTime.Now ? "Overdue" : "On Time")
                });

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                issuedBooksQuery = issuedBooksQuery.Where(ib =>
                    ib.BookTitle.Contains(searchQuery) ||
                    ib.StudentName.Contains(searchQuery));
            }

            var filteredIssuedBooks = issuedBooksQuery.ToList();
            return PartialView("ViewIssuedBooks", filteredIssuedBooks); // Return filtered results
        }
        [HttpGet]
        public IActionResult ManageFines()
        {
            try
            {
                // Get all overdue issued books that are not returned
                var overdueBooks = _context.IssuedBooks
                    .Where(ib => ib.DueDate < DateTime.Now && ib.ReturnDate == null)
                    .Include(ib => ib.Book)
                    .Include(ib => ib.User)
                    .ToList();
                // Create fines for overdue books if they don't already have a fine
                foreach (var issuedBook in overdueBooks)
                {
                    if (!_context.Fines.Any(f => f.IssuedBookId == issuedBook.IssuedBookId))
                    {
                        int overdueDays = (DateTime.Now.Date - issuedBook.DueDate.Date).Days;
                        double fineAmount = overdueDays * 10; // Rs.10 per day
                        var fine = new Fine
                        {
                            IssuedBookId = issuedBook.IssuedBookId,
                            FineAmount = fineAmount,
                            FineDate = DateTime.Now,
                            IsPaid = false
                        };
                        _context.Fines.Add(fine);
                    }
                }
                _context.SaveChanges(); // Save changes to DB
                UpdateMetrics();
                // Get all fines (both paid and unpaid)
                var fines = _context.Fines
                    .Include(f => f.IssuedBook)
                    .ThenInclude(ib => ib.Book)
                    .Include(f => f.IssuedBook.User)
                    .Select(f => new
                    {
                        f.FineId,
                        StudentName = f.IssuedBook.User.FullName,
                        BookTitle = f.IssuedBook.Book.Title,
                        f.FineAmount,
                        DueDate = f.IssuedBook.DueDate,
                        ReturnDate = f.IssuedBook.ReturnDate,
                        IsPaid = f.IsPaid
                    })
                    .ToList();
                return PartialView("ManageFines", fines);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error loading ManageFines view: {ex.Message}" });
            }
        }
        [HttpPost]
        public IActionResult PayFine(int fineId)
        {
            var fine = _context.Fines.Include(f => f.IssuedBook).FirstOrDefault(f => f.FineId == fineId);
            if (fine == null)
                return Json(new { success = false, message = "Fine not found." });
            // Mark fine as paid
            fine.IsPaid = true;
            // Update the return date of the issued book
            var issuedBook = fine.IssuedBook;
            if (issuedBook != null)
            {
                issuedBook.ReturnDate = DateTime.Now; // Set today's date as the return date
                var bookRequest = _context.BookRequests.FirstOrDefault(r => r.BookId == issuedBook.BookId
                                                                         && r.UserId==issuedBook.UserId
                                                                         && r.Status=="Approved");
                if (bookRequest != null)
                {
                    bookRequest.Status = "Returned";
                }
            }
            _context.SaveChanges(); // Save changes to the database
            // Update metrics after marking the fine as paid
            var metrics = GetMetrics();
            return Json(new { success = true, message = "Fine marked as paid successfully!", metrics });
        }
        [HttpGet]
        public IActionResult ReturnBook()
        {
            try
            {
                var issuedBooks = _context.IssuedBooks
                    .Where(ib => ib.ReturnDate == null) // Show books that are not returned
                    .Include(ib => ib.Book)
                    .Include(ib => ib.User)
                    .Select(ib => new
                    {
                        ib.IssuedBookId,
                        BookTitle = ib.Book.Title,
                        StudentName = ib.User.FullName,
                        IssueDate = ib.IssueDate,
                        DueDate = ib.DueDate
                    })
                    .ToList();

                return PartialView("ReturnBook", issuedBooks); // Load the ReturnBook view
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error loading Return Book view: {ex.Message}" });
            }
        }
        [HttpPost]
        public IActionResult ReturnBook(int issuedBookId)
        {
            try
            {
                var issuedBook = _context.IssuedBooks
                    .Include(ib => ib.Book)
                    .Include(ib => ib.User)
                    .FirstOrDefault(ib => ib.IssuedBookId == issuedBookId);

                if (issuedBook == null)
                    return Json(new { success = false, message = "Issued book not found." });

                bool isOverdue = issuedBook.DueDate < DateTime.Now && issuedBook.ReturnDate == null;

                if (isOverdue)
                {
                    return Json(new { success = false, redirectTo = "/Admin/ManageFines", message = "This book is overdue. Redirecting to manage fines." });
                }

                // Mark the book as returned
                issuedBook.ReturnDate = DateTime.Now;

                // Update the corresponding book request status to "Returned"
                var bookRequest = _context.BookRequests.FirstOrDefault(r =>
                    r.BookId == issuedBook.BookId &&
                    r.UserId == issuedBook.UserId &&
                    r.Status == "Approved");  // Only update the approved request

                if (bookRequest != null)
                {
                    bookRequest.Status = "Returned";
                }

                _context.SaveChanges();  // Save changes to the database

                var metrics = GetMetrics();  // Update metrics
                return Json(new { success = true, message = "Book returned successfully!", metrics });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error processing return: {ex.Message}" });
            }
        }




        public IActionResult GenerateReports()
        {
            return PartialView("GenerateReports");
        }

        private void UpdateMetrics()
        {
            ViewBag.TotalBooks = _context.Books.Count();
            ViewBag.TotalStudents = _context.Users.Count(u => u.Role == "Student");
            ViewBag.TotalIssuedBooks = _context.IssuedBooks.Count(ib => ib.ReturnDate == null);
            ViewBag.TotalOverdueBooks = _context.IssuedBooks.Count(ib => ib.DueDate < DateTime.Now && ib.ReturnDate == null);
        }

        private object GetMetrics()
        {
            return new
            {
                TotalBooks = _context.Books.Count(),
                TotalStudents = _context.Users.Count(u => u.Role == "Student"),
                TotalIssuedBooks = _context.IssuedBooks.Count(ib => ib.ReturnDate == null),
                TotalOverdueBooks = _context.IssuedBooks.Count(ib => ib.DueDate < DateTime.Now && ib.ReturnDate == null)
            };
        }
    }
}
