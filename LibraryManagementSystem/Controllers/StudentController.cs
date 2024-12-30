using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class StudentController : Controller
{
    private readonly LibraryContext _context;

    public StudentController(LibraryContext context)
    {
        _context = context;
    }

    // Student Dashboard
    public IActionResult Dashboard()
    {
        // Retrieve the userId from the session
        var userId = HttpContext.Session.GetInt32("UserId");

        if (!userId.HasValue)
        {
            return RedirectToAction("Index", "Home"); // Redirect to home if not logged in
        }

        // Check the user's role
        var role = HttpContext.Session.GetString("Role");
        if (role != "Student")
        {
            return Unauthorized(); // Deny access if the user is not a student
        }

        // Populate dashboard metrics
        ViewBag.TotalBooks = _context.Books.Count();
        ViewBag.BooksIssued = _context.IssuedBooks.Count(i => i.UserId == userId.Value);
        ViewBag.OverdueFines = _context.Fines
            .Include(f => f.IssuedBook)
            .Where(f => !f.IsPaid && f.IssuedBook.UserId == userId.Value)
            .Sum(f => (decimal?)f.FineAmount) ?? 0;

        return View();
    }

    // Fetch all books for Search Books functionality
    public IActionResult SearchBooks(string query)
    {
        var books = string.IsNullOrEmpty(query)
            ? _context.Books.ToList()
            : _context.Books.Where(b => b.Title.Contains(query) || b.Author.Contains(query) || b.Genre.Contains(query)).ToList();

        return PartialView("_SearchBooks", books);
    }

    // View Issued Books
    public IActionResult ViewIssuedBooks()
    {
        var userId = HttpContext.Session.GetInt32("UserId");

        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var issuedBooks = _context.IssuedBooks
            .Include(ib => ib.Book)
            .Where(ib => ib.UserId == userId.Value)
            .ToList();

        return PartialView("_ViewIssuedBooks", issuedBooks);
    }

    // View Fines
    public IActionResult ViewFines()
    {
        var userId = HttpContext.Session.GetInt32("UserId");

        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var fines = _context.Fines
            .Include(f => f.IssuedBook.Book)
            .Where(f => f.IssuedBook.UserId == userId.Value && !f.IsPaid)
            .ToList();

        return PartialView("_ViewFines", fines);
    }

    // Renew Book
    // Renew Book
    [HttpGet]
    public IActionResult RenewBooks()
    {
        var userId = HttpContext.Session.GetInt32("UserId");

        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var issuedBooks = _context.IssuedBooks
            .Include(ib => ib.Book)
            .Where(ib => ib.UserId == userId.Value && ib.ReturnDate == null && DateTime.Now <= ib.DueDate)
            .ToList();

        return PartialView("_RenewBooks", issuedBooks);
    }

    [HttpPost]
    public IActionResult RenewBook(int issuedBookId)
    {
        var issuedBook = _context.IssuedBooks.Include(ib => ib.Book).FirstOrDefault(ib => ib.IssuedBookId == issuedBookId);
        if (issuedBook != null && issuedBook.ReturnDate == null && DateTime.Now <= issuedBook.DueDate)
        {
            issuedBook.DueDate = issuedBook.DueDate.AddDays(7); // Extend due date by 7 days
            _context.SaveChanges();
            return Ok("Book renewed successfully.");
        }
        return BadRequest("Unable to renew the book.");
    }

    [HttpPost]
    public IActionResult RequestBook(int bookId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (!userId.HasValue)
            return Unauthorized();

        var book = _context.Books.FirstOrDefault(b => b.BookId == bookId && b.AvailableCopies > 0);
        if (book == null)
            return BadRequest("The book is unavailable.");

        var existingRequest = _context.BookRequests.FirstOrDefault(r => r.BookId == bookId && r.UserId == userId.Value && r.Status == "Pending");
        if (existingRequest != null)
            return BadRequest("You have already requested this book.");

        var request = new BookRequest
        {
            BookId = bookId,
            UserId = userId.Value,
            RequestDate = DateTime.Now,
            Status = "Pending"
        };

        _context.BookRequests.Add(request);
        _context.SaveChanges();

        TempData["SuccessMessage"] = "Book request submitted successfully!";
        return RedirectToAction("SearchBooks");
    }

    // Return Book
    [HttpGet]
    public IActionResult ReturnBooks()
    {
        var userId = HttpContext.Session.GetInt32("UserId");

        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var issuedBooks = _context.IssuedBooks
            .Include(ib => ib.Book)
            .Where(ib => ib.UserId == userId.Value && ib.ReturnDate == null)
            .ToList();

        return PartialView("_ReturnBooks", issuedBooks);
    }

    [HttpPost]
    public IActionResult ReturnBook(int issuedBookId)
    {
        var issuedBook = _context.IssuedBooks.Include(ib => ib.Book).FirstOrDefault(ib => ib.IssuedBookId == issuedBookId);
        if (issuedBook != null && issuedBook.ReturnDate == null)
        {
            issuedBook.ReturnDate = DateTime.Now; // Mark as returned
            issuedBook.Book.AvailableCopies += 1; // Increment the available copies
            _context.SaveChanges();
            return Ok("Book returned successfully.");
        }
        return BadRequest("Unable to return the book.");
    }
}
