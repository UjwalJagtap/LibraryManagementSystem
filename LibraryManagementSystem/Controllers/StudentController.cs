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
        return PartialView("_Books", books); // Render the list of all books
    }

    [HttpPost]
    public IActionResult Books(string searchQuery)
    {
        var books = string.IsNullOrWhiteSpace(searchQuery)
            ? _context.Books.ToList() // Return all books if no query
            : _context.Books.Where(b => b.Title.Contains(searchQuery) ||
                                        b.Author.Contains(searchQuery) ||
                                        b.Genre.Contains(searchQuery)).ToList(); // Filter by title, author, or genre

        return PartialView("_Books", books); // Return updated list in the same partial view
    }


    [HttpPost]
    public IActionResult RequestBook(int bookId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (!userId.HasValue)
            return Json(new { success = false, message = "User not logged in." });

        var book = _context.Books.FirstOrDefault(b => b.BookId == bookId);
        if (book == null || book.AvailableCopies <= 0)
            return Json(new { success = false, message = "Book is unavailable." });

        // Check if there is an existing issued book or pending request
        var existingIssuedBook = _context.IssuedBooks.FirstOrDefault(ib => ib.BookId == bookId && ib.UserId == userId.Value && ib.ReturnDate == null);
        var existingRequest = _context.BookRequests.FirstOrDefault(r => r.BookId == bookId && r.UserId == userId.Value && r.Status == "Pending");

        if (existingIssuedBook != null)
            return Json(new { success = false, message = "You have already issued this book." });

        if (existingRequest != null)
            return Json(new { success = false, message = "You have already requested this book." });

        // Add new book request
        var bookRequest = new BookRequest
        {
            UserId = userId.Value,
            BookId = bookId,
            RequestDate = DateTime.Now,
            Status = "Pending"
        };

        book.AvailableCopies -= 1; // Decrease available copies
        _context.BookRequests.Add(bookRequest);
        _context.SaveChanges();

        var updatedMetrics = GetStudentMetrics(userId.Value);
        return Json(new { success = true, message = "Book request submitted successfully!", updatedMetrics });
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

    return PartialView("_ViewBookRequests", bookRequests);
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
    public IActionResult ClearCancelledRequests()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (!userId.HasValue)
            return Json(new { success = false, message = "User not logged in." });

        var cancelledRequests = _context.BookRequests.Where(r => r.UserId == userId.Value && r.Status == "Cancelled").ToList();
        if (!cancelledRequests.Any())
            return Json(new { success = false, message = "No cancelled requests to clear." });

        _context.BookRequests.RemoveRange(cancelledRequests);
        _context.SaveChanges();

        return Json(new { success = true, message = "All cancelled requests cleared successfully!" });
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
