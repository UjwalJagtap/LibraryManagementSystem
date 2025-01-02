using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    public class Book
    {
        [Key]
        public int BookId { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Author is required.")]
        public string Author { get; set; }

        [Required(ErrorMessage = "Publisher is required.")]
        public string Publisher { get; set; }

        [Required(ErrorMessage = "ISBN is required.")]
        public string ISBN { get; set; }

        [Required(ErrorMessage = "Published Year is required.")]
        public int PublishedYear { get; set; }

        [Required(ErrorMessage = "Genre is required.")]
        public string Genre { get; set; }

        [Required(ErrorMessage = "Total Copies are required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Total copies must be at least 1.")]
        public int TotalCopies { get; set; }

        [Required(ErrorMessage = "Available Copies are required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Available copies cannot be negative.")]
        public int AvailableCopies { get; set; }
    }
}
