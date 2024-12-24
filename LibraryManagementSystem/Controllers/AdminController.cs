using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;

public class AdminController : Controller
{
    private readonly LibraryContext _context;

    public AdminController(LibraryContext context)
    {
        _context = context;
    }

    public IActionResult Dashboard()
    {
        // Pass metrics to the view
        ViewBag.TotalBooks = _context.Books.Count();
        ViewBag.TotalStudents = _context.Users.Count(u => u.Role == "Student");
        ViewBag.TotalIssuedBooks = _context.IssuedBooks.Count();
        ViewBag.TotalOverdueBooks = _context.IssuedBooks.Count(ib => ib.DueDate < DateTime.Now && ib.ReturnDate == null);

        return View();
    }

    public IActionResult AddBooks()
    {
        return PartialView("_AddBooks");
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
}
