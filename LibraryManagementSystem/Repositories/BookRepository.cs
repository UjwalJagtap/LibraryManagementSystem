using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly LibraryContext _context;

        public BookRepository(LibraryContext context)
        {
            _context = context;
        }

        public IEnumerable<Book> GetAllBooks() => _context.Books.ToList();

        public Book GetBookById(int id) => _context.Books.Find(id);

        public void AddBook(Book book) => _context.Books.Add(book);

        public void UpdateBook(Book book) => _context.Books.Update(book);

        public void DeleteBook(int id)
        {
            var book = _context.Books.Find(id);
            if (book != null)
            {
                _context.Books.Remove(book);
            }
        }

        public bool SaveChanges() => _context.SaveChanges() > 0;
    }
}
