using LibraryManagementSystem.Data;
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

        // Ensure the user is authenticated by checking if userId is available
        if (!userId.HasValue)
        {
            return Unauthorized(); // If no userId in session, return Unauthorized
        }

        // You can also check the role if necessary (example for Student)
        var role = HttpContext.Session.GetString("Role");
        if (role != "Student")
        {
            return Unauthorized(); // If the user is not a student, deny access
        }

        // Add data for dashboard metrics
        ViewBag.TotalBooks = _context.Books.Count(); // Total number of books in the library
        ViewBag.BooksIssued = _context.IssuedBooks.Count(i => i.UserId == userId.Value); // Count of books issued by the student
        ViewBag.OverdueFines = _context.Fines
            .Include(f => f.IssuedBook) // Include the related IssuedBook information
            .Where(f => !f.IsPaid && f.IssuedBook.UserId == userId.Value) // Only unpaid fines for the logged-in user
            .Sum(f => (decimal?)f.FineAmount) ?? 0; // Sum of overdue fines (defaulting to 0 if null)

        // You can add more metrics here as needed

        return View();
    }


    // Fetch all books for Search Books functionality
    public IActionResult SearchBooks()
    {
        var books = _context.Books.ToList();
        return PartialView("_SearchBooks", books);
    }

    // View Issued Books
    public IActionResult ViewIssuedBooks()
    {
        var userId = HttpContext.Session.GetInt32("UserId");

        if (!userId.HasValue)
        {
            return Unauthorized(); // Handle unauthorized access if UserId is not valid
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
            return Unauthorized(); // Handle unauthorized access if UserId is not valid
        }

        var fines = _context.Fines
            .Include(f => f.IssuedBook.Book)
            .Where(f => f.IssuedBook.UserId == userId.Value && !f.IsPaid)
            .ToList();

        return PartialView("_ViewFines", fines);
    }

    // Renew Book
    [HttpPost]
    public IActionResult RenewBook(int issuedBookId)
    {
        var issuedBook = _context.IssuedBooks.Find(issuedBookId);
        if (issuedBook != null && issuedBook.ReturnDate == null && DateTime.Now <= issuedBook.DueDate)
        {
            issuedBook.DueDate = issuedBook.DueDate.AddDays(7); // Extend due date by 7 days
            _context.SaveChanges();
            return Ok();
        }
        return BadRequest("Unable to renew the book.");
    }

    // Return Book
    [HttpPost]
    public IActionResult ReturnBook(int issuedBookId)
    {
        var issuedBook = _context.IssuedBooks.Find(issuedBookId);
        if (issuedBook != null && issuedBook.ReturnDate == null)
        {
            issuedBook.ReturnDate = DateTime.Now; // Mark as returned
            _context.SaveChanges();
            return Ok();
        }
        return BadRequest("Unable to return the book.");
    }
}
