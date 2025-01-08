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
        ViewBag.BooksIssued = _context.IssuedBooks.Count(i => i.UserId == userId.Value);
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

        // Check if the book already exists in IssuedBooks
        bool alreadyIssued = _context.IssuedBooks.Any(ib => ib.BookId == bookId && ib.UserId == userId.Value && ib.ReturnDate == null);
        if (alreadyIssued)
            return Json(new { success = false, message = "This book is already issued to you." });

        // Add a new request to the BookRequests table
        var bookRequest = new BookRequest
        {
            UserId = userId.Value,
            BookId = bookId,
            RequestDate = DateTime.Now,
            Status = "Pending",
            RequestType = "New" // Request type for new book requests
        };

        _context.BookRequests.Add(bookRequest);
        _context.SaveChanges();

        return Json(new { success = true, message = "Your book request has been submitted!" });
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

        // Check if a renewal request already exists for the same book
        bool renewalPending = _context.BookRequests.Any(br => br.BookId == issuedBook.BookId && br.UserId == userId.Value && br.RequestType == "Renewal" && br.Status == "Pending");
        if (renewalPending)
            return Json(new { success = false, message = "Renewal request already submitted for this book." });

        // Add a renewal request to the BookRequests table
        var renewalRequest = new BookRequest
        {
            UserId = userId.Value,
            BookId = issuedBook.BookId,
            RequestDate = DateTime.Now,
            Status = "Pending",
            RequestType = "Renewal" // Request type for renewals
        };

        _context.BookRequests.Add(renewalRequest);
        _context.SaveChanges();

        return Json(new { success = true, message = "Renewal request has been submitted successfully!" });
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
        var userId = HttpContext.Session.GetInt32("UserId");
        if (!userId.HasValue)
            return RedirectToAction("Login", "Account");

        var fines = _context.Fines
            .Include(f => f.IssuedBook)
            .ThenInclude(ib => ib.Book)
            .Where(f => f.IssuedBook.UserId == userId.Value && !f.IsPaid)
            .Select(f => new
            {
                f.FineId,
                BookTitle = f.IssuedBook.Book.Title,
                f.FineAmount,
                f.FineDate,
                f.IsPaid
            }).ToList();

        return PartialView("ViewFines", fines);
    }


    [HttpPost]
    public IActionResult PayFine(int fineId)
    {
        var fine = _context.Fines.FirstOrDefault(f => f.FineId == fineId);
        if (fine == null)
            return Json(new { success = false, message = "Fine not found." });

        fine.IsPaid = true; // Mark fine as paid
        _context.SaveChanges();

        return Json(new { success = true, message = "Fine paid successfully!" });
    }



    private object GetStudentMetrics(int userId)
    {
        return new
        {
            TotalBooks = _context.Books.Count(),
            BooksIssued = _context.IssuedBooks.Count(ib => ib.UserId == userId),
            OverdueFines = _context.Fines
                .Include(f => f.IssuedBook)
                .Where(f => !f.IsPaid && f.IssuedBook.UserId == userId)
                .Sum(f => (decimal?)f.FineAmount) ?? 0
        };
    }
}
