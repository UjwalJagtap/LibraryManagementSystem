using Microsoft.EntityFrameworkCore;
using System;

namespace LibraryManagementSystem.Models
{
    public class LibraryContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<IssuedBook> IssuedBooks { get; set; }
        public DbSet<Fine> Fines { get; set; }
        public DbSet<StudentReport> StudentReports { get; set; }
        public DbSet<BookRequest> BookRequests { get; set; } // If applicable

        public LibraryContext(DbContextOptions<LibraryContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seeding default admin user
            modelBuilder.Entity<User>().HasData(new User
            {
                UserId = 1,
                Username = "admin",
                PasswordHash = HashPassword("Admin123"),
                FullName = "Library Admin",
                Email = "admin@library.com",
                Phone = "1234567890",
                Role = "Librarian"
            });

            // Optionally, seed additional tables like Books, Fines, etc.
        }

        private static string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }
}
