using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TravelApp.Models;

namespace TravelApp.Models
{
    public class ReviewComment
    {
        [ForeignKey("Review")]
        public int Review_ID { get; set; }

        [Key]
        public int Comment_ID { get; set; }

        [Required]
        [StringLength(1000)]
        public string Comment_Text { get; set; }

        public DateTime Comment_Date { get; set; }

        // Navigation properties
        public Review Review { get; set; }
    }
}
