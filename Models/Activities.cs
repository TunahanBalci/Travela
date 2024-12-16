using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TravelApp.Models;

namespace TravelApp.Models
{
    public class Activities
    {
        [Key]
        public int Activity_ID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(50)]
        public string Type { get; set; }

        [StringLength(100)]
        public string Schedule { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        public bool Required_Reservations { get; set; }

        [Column(TypeName = "decimal(3,2)")]
        public decimal Average_Review { get; set; }

        [ForeignKey("Destination")]
        public int Destination_ID { get; set; }

        // Navigation properties
        public Destination Destination { get; set; }
        public ICollection<AccommodationActivity> AccommodationActivities { get; set; }
    }
}
