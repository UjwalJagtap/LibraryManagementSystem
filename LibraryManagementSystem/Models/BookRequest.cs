using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LibraryManagementSystem.Models
{
    public class BookRequest
    {
        [Key]
        public int RequestId { get; set; }

        
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        [JsonIgnore]
        [ValidateNever]
        public User User { get; set; }

        [ForeignKey("Book")]
        public int BookId { get; set; }
        [JsonIgnore]
        [ValidateNever]
        public Book Book { get; set; }

        public DateTime RequestDate { get; set; }
        public string Status { get; set; } // Pending, Approved, Rejected
        public string RequestType { get; set; }

    }
}
