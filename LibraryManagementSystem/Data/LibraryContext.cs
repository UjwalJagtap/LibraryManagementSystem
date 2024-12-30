using Microsoft.EntityFrameworkCore;
using System;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Data

{
    public class LibraryContext : DbContext
    {
        // Define DbSet properties for each table
        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<IssuedBook> IssuedBooks { get; set; }
        public DbSet<Fine> Fines { get; set; }
        public DbSet<StudentReport> StudentReports { get; set; }
        public DbSet<BookRequest> BookRequests { get; set; }

        public LibraryContext(DbContextOptions<LibraryContext> options) : base(options) { }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed default admin user
            modelBuilder.Entity<User>().HasData(new User
            {
                UserId = 1,
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123"),
                FullName = "Library Admin",
                Email = "admin@library.com",
                Phone = "9234567810",
                Role = "Librarian"
            });
            // Optional: Add additional table seeds here if needed
        }

        //<summary>
        // Hashes the provided password using SHA256.
        // </summary>
        // <param name="password">Password to hash.</param>
        // <returns>SHA256 hashed password as a string.</returns>
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
