using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TravelApp.Models;

namespace TravelApp.Models
{
    public class UserPreference
    {
        [ForeignKey("User")]
        public int User_ID { get; set; }

        [Key]
        [StringLength(100)]
        public string Preference { get; set; }

        // Navigation properties
        public User User { get; set; }
    }
}
