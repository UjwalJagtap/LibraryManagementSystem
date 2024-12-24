using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models
{
    public class StudentReport
    {
        [Key]
        public int ReportId { get; set; }

        [ForeignKey("User")]
        [Required(ErrorMessage = "User reference is required.")]
        public int UserId { get; set; }
        public User User { get; set; }

        [Required(ErrorMessage = "Report date is required.")]
        public DateTime ReportDate { get; set; }

        [Required(ErrorMessage = "Report details are required.")]
        [MaxLength(1000, ErrorMessage = "Report details cannot exceed 1000 characters.")]
        public string ReportDetails { get; set; }
    }
}
