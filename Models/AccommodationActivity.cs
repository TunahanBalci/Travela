using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using TravelApp.Models;

namespace TravelApp.Models
{
    public class AccommodationActivity
    {
        [ForeignKey("Accommodation")]
        public int Accommodation_ID { get; set; }
        [Key]
        [ForeignKey("Activity")]
        public int Activity_ID { get; set; }

        // Navigation properties
        public Accommodation Accommodation { get; set; }
        public Activities Activity { get; set; }
    }
}
