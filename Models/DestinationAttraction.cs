using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelApp.Models
{
    public class DestinationAttraction
    {
        [ForeignKey("Destination")]
        public int Destination_ID { get; set; }

        [Key]
        [StringLength(255)]
        public string Attraction { get; set; }

        // Navigation properties
        public Destination Destination { get; set; }
    }
}
