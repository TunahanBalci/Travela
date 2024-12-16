using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TravelApp.Models
{
    public class User
    {
        [Key]
        public int User_ID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(255)]
        public string Password { get; set; }

        // Navigation properties
        public ICollection<Booking> Bookings { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<UserDestination> UserDestinations { get; set; }
        // Add other navigation properties as needed
    }
}
