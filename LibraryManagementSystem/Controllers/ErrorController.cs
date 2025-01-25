using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        [HttpGet] // Explicitly define the HTTP method
        public IActionResult HandleError(int statusCode)
        {
            ViewData["ErrorMessage"] = statusCode switch
            {
                404 => "Page not found.",
                500 => "Server error occurred.",
                _ => "An unexpected error occurred."
            };

            return View("Error");
        }

        [Route("Error")]
        [HttpGet] // Explicitly define the HTTP method for Swagger
        public IActionResult Error()
        {
            return View();
        }
    }
}
