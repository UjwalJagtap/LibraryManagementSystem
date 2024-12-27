using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    public class Fine
    {
        [Key]
        public int FineId { get; set; }

        [ForeignKey("IssuedBook")]
        [Required(ErrorMessage = "Issued Book reference is required.")]
        public int IssuedBookId { get; set; }
        public IssuedBook IssuedBook { get; set; }

        [Required(ErrorMessage = "Fine amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Fine amount must be greater than zero.")]
        public double FineAmount { get; set; }

        [Required(ErrorMessage = "Fine date is required.")]
        public DateTime FineDate { get; set; }

        public bool IsPaid { get; set; }
    }
}
