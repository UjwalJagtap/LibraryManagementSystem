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
        }
    }
}
