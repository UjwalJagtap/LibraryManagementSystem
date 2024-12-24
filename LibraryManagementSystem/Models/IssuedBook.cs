using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models
{
    public class IssuedBook
    {
        [Key]
        public int IssuedBookId { get; set; }

        [ForeignKey("Book")]
        [Required(ErrorMessage = "Book reference is required.")]
        public int BookId { get; set; }
        public Book Book { get; set; }

        [ForeignKey("User")]
        [Required(ErrorMessage = "User reference is required.")]
        public int UserId { get; set; }
        public User User { get; set; }

        [Required(ErrorMessage = "Issue date is required.")]
        public DateTime IssueDate { get; set; }

        [Required(ErrorMessage = "Due date is required.")]
        public DateTime DueDate { get; set; }

        public DateTime? ReturnDate { get; set; } // Nullable for books not yet returned
    }
}
