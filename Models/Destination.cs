using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TravelApp.Models;

namespace TravelApp.Models
{
    public class Destination
    {
        [Key]
        public int Destination_ID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        public string Location { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [Column(TypeName = "decimal(3,2)")]
        public decimal Average_Review { get; set; }

        [ForeignKey("City")]
        public int City_ID { get; set; }

        // Navigation properties
        public City City { get; set; }
        public ICollection<DestinationAttraction> DestinationAttractions { get; set; }
        public ICollection<UserDestination> UserDestinations { get; set; }
        // Add other navigation properties as needed
    }
}
