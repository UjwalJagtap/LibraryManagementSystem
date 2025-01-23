using Microsoft.AspNetCore.Mvc;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Data;

namespace LibraryManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.HeaderController = "Home";
            ViewBag.HeaderAction = "Index";
            return View();
        }

    }
}
