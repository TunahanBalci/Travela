using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TravelApp.Models;

namespace TravelApp.Models
{
    public class Review
    {
        [Key]
        public int Review_ID { get; set; }

        [ForeignKey("User")]
        public int User_ID { get; set; }

        [Required]
        [StringLength(20)]
        public string Entity_Type { get; set; } // 'Destination', 'Accommodation', 'Activity'

        public int Entity_ID { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        // Navigation properties
        public User User { get; set; }
        public ICollection<ReviewComment> ReviewComments { get; set; }
        // Add other navigation properties as needed
    }
}
