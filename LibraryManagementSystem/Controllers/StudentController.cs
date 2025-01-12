using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

public class StudentController : Controller
{
    private readonly LibraryContext _context;

    public StudentController(LibraryContext context)
    {
        _context = context;
    }
    public IActionResult Dashboard()
    {
        var userId = HttpContext.Session.GetInt32("UserId");

        // Redirect to home if not logged in
        if (!userId.HasValue)
            return RedirectToAction("Index", "Home");

        // Check the user's role
        var role = HttpContext.Session.GetString("Role");
        if (role != "Student")
            return Unauthorized();
 

        // Populate dashboard metrics
        ViewBag.TotalBooks = _context.Books.Count();
        ViewBag.BooksIssued = _context.IssuedBooks.Count(i => i.UserId == userId.Value && i.ReturnDate == null);
        ViewBag.OverdueFines = _context.Fines
            .Include(f => f.IssuedBook)
            .Where(f => !f.IsPaid && f.IssuedBook.UserId == userId.Value)
            .Sum(f => (decimal?)f.FineAmount) ?? 0;

        return View("Dashboard"); // Return the Dashboard view
    }

    // Display all books with search functionality
    public IActionResult Books()
    {
        var books = _context.Books.ToList(); // Fetch all books initially
        return PartialView("Books", books); // Render the list of all books
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

    [HttpPost]
    public IActionResult RequestBook(int bookId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (!userId.HasValue)
            return Json(new { success = false, message = "User not logged in." });

        // Check if a pending or approved request already exists for the same book and user
        var requests = _context.BookRequests.Where(r => r.BookId == bookId && r.UserId == userId.Value).ToList();

        foreach (var request in requests)
        {
            Console.WriteLine($"Request ID: {request.RequestId}, Status: {request.Status}");
        }
        bool requestExists = _context.BookRequests.Any(r =>
            r.BookId == bookId &&
            r.UserId == userId.Value &&
            (r.Status == "Pending" || r.Status == "Approved"));

        if (requestExists)
        {
            return Json(new { success = false, message = "You have already requested or issued this book." });
        }

        var newRequest = new BookRequest
        {
            BookId = bookId,
            UserId = userId.Value,
            RequestDate = DateTime.Now,
            Status = "Pending",
            RequestType = "New"
        };

        _context.BookRequests.Add(newRequest);
        _context.SaveChanges();

        return Json(new { success = true, message = "Book request submitted successfully!" });
    }
    [HttpPost]
    public IActionResult RenewBook(int issuedBookId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (!userId.HasValue)
            return Json(new { success = false, message = "User not logged in." });

        var issuedBook = _context.IssuedBooks.FirstOrDefault(ib => ib.IssuedBookId == issuedBookId && ib.UserId == userId.Value);
        if (issuedBook == null)
            return Json(new { success = false, message = "Issued book not found." });

        // Check if a pending or approved renewal request already exists for the same book
        bool renewalExists = _context.BookRequests.Any(r =>
            r.BookId == issuedBook.BookId &&
            r.UserId == userId.Value &&
            r.RequestType == "Renewal" &&
            (r.Status == "Pending" || r.Status == "Approved"));

        if (renewalExists)
        {
            return Json(new { success = false, message = "You have already requested a renewal for this book." });
        }

        // Add new renewal request
        var renewalRequest = new BookRequest
        {
            BookId = issuedBook.BookId,
            UserId = userId.Value,
            RequestDate = DateTime.Now,
            Status = "Pending",
            RequestType = "Renewal"
        };

        _context.BookRequests.Add(renewalRequest);
        _context.SaveChanges();

        return Json(new { success = true, message = "Renewal request submitted successfully!" });
    }

    public IActionResult ViewBookRequests()
    {
    var userId = HttpContext.Session.GetInt32("UserId");
    if (!userId.HasValue)
        return Unauthorized();

    var bookRequests = _context.BookRequests
        .Include(br => br.Book)  // Eagerly load the Book entity to avoid null references
        .Where(br => br.UserId == userId.Value)
        .OrderByDescending(br => br.RequestDate)
        .ToList();

    return PartialView("ViewBookRequests", bookRequests);
    }


    [HttpPost]
    public IActionResult CancelRequest(int requestId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (!userId.HasValue)
            return Json(new { success = false, message = "User not logged in." });

        var request = _context.BookRequests.Include(r => r.Book).FirstOrDefault(r => r.RequestId == requestId && r.UserId == userId.Value);
        if (request == null || request.Status != "Pending")
            return Json(new { success = false, message = "Request not found or already processed." });

        // Update the status to "Cancelled"
        request.Status = "Cancelled";
        request.Book.AvailableCopies += 1; // Increment the available copies
        _context.SaveChanges();

        return Json(new { success = true, message = "Request cancelled successfully!" });
    }
    [HttpPost]
    public IActionResult ClearAllRequests()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (!userId.HasValue)
            return Json(new { success = false, message = "User not logged in." });

        var allRequests = _context.BookRequests.Where(r => r.UserId == userId.Value).ToList();
        if (!allRequests.Any())
            return Json(new { success = false, message = "No requests to clear." });

        _context.BookRequests.RemoveRange(allRequests);
        _context.SaveChanges();

        return Json(new { success = true, message = "All requests cleared successfully!" });
    }
    public IActionResult ViewIssuedBooks()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (!userId.HasValue)
            return RedirectToAction("Index", "Home");

        var issuedBooks = _context.IssuedBooks
            .Where(ib => ib.UserId == userId.Value && ib.ReturnDate == null)
            .Select(ib => new
            {
                ib.IssuedBookId,
                BookTitle = ib.Book.Title,
                Author = ib.Book.Author,
                ib.IssueDate,
                ib.DueDate,
                ib.ReturnDate
            })
            .ToList();

        return PartialView("ViewIssuedBooks", issuedBooks);
    }
    [HttpGet]
    public IActionResult ViewFines()
    {
        try
        {
            // Get the logged-in student ID from session
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Home");
            }

            // Get all fines related to the student
            var fines = _context.Fines
                .Include(f => f.IssuedBook)
                .ThenInclude(ib => ib.Book)
                .Where(f => f.IssuedBook.UserId == userId.Value)
                .Select(f => new
                {
                    f.FineId,
                    BookTitle = f.IssuedBook.Book.Title,
                    f.FineAmount,
                    FineDate = f.FineDate,
                    DueDate = f.IssuedBook.DueDate,
                    ReturnDate = f.IssuedBook.ReturnDate,
                    Status = f.IsPaid ? "Paid" : "Unpaid"
                })
                .ToList();

            return PartialView("ViewFines", fines);
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Error loading fines: {ex.Message}" });
        }
    }

    private object GetStudentMetrics(int userId)
    {
        return new
        {
            TotalBooks = _context.Books.Count(),
            BooksIssued = _context.IssuedBooks.Count(ib => ib.UserId == userId && ib.ReturnDate == null),
            OverdueFines = _context.Fines
                .Include(f => f.IssuedBook)
                .Where(f => !f.IsPaid && f.IssuedBook.UserId == userId)
                .Sum(f => (decimal?)f.FineAmount) ?? 0
        };
    }
}
