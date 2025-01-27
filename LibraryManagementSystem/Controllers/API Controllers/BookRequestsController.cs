using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookRequestsController : ControllerBase
    {
        private readonly LibraryContext _context;

        public BookRequestsController(LibraryContext context)
        {
            _context = context;
        }

        // GET: api/BookRequests
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookRequest>>> GetBookRequests()
        {
            return await _context.BookRequests.Include(r => r.Book).Include(r => r.User).ToListAsync();
        }

        // GET: api/BookRequests/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BookRequest>> GetBookRequest(int id)
        {
            var bookRequest = await _context.BookRequests.FindAsync(id);

            if (bookRequest == null)
            {
                return NotFound();
            }

            return bookRequest;
        }

        // PUT: api/BookRequests/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [HttpPut]
        public async Task<IActionResult> PutBookRequest(BookRequest bookRequest)
        {
           

            _context.Entry(bookRequest).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookRequestExists(bookRequest.RequestId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/BookRequests
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<BookRequest>> PostBookRequest(BookRequest bookRequest)
        {
            _context.BookRequests.Add(bookRequest);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBookRequest", new { id = bookRequest.RequestId }, bookRequest);
        }

        // DELETE: api/BookRequests/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBookRequest(int id)
        {
            var bookRequest = await _context.BookRequests.FindAsync(id);
            if (bookRequest == null)
            {
                return NotFound();
            }

            _context.BookRequests.Remove(bookRequest);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        private bool UserIsAdmin()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Librarian";
        }
        // GET: api/BookRequests/count/pending
        [HttpGet("count/pending")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> GetPendingRequestCount()
        {
            if (!UserIsAdmin())
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    success = false,
                    message = "Access denied. Admins only."
                });
            }
            int pendingCount = await _context.BookRequests
                .CountAsync(r => r.Status == "pending");

            return Ok(new
            {
                success = true,
                count = pendingCount
            });
        }
        // DELETE: api/BookRequests/status/{status}
        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<BookRequest>>> GetBookRequestsByStatus(string status)
        {
            var bookRequests = await _context.BookRequests
                .Include(r => r.Book)
                .Include(r => r.User)
                .Where(r => r.Status == status)
                .ToListAsync();

            if (bookRequests == null || !bookRequests.Any())
            {
                return NotFound(new { Message = $"No book requests found with status '{status}'." });
            }

            return bookRequests;
        }

        private bool BookRequestExists(int id)
        {
            return _context.BookRequests.Any(e => e.RequestId == id);
        }
    }
}
