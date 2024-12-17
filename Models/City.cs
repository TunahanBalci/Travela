using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelApp.Models
{
    public class City
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int City_ID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        public string Location { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Cost_Of_Living { get; set; }

        [StringLength(50)]
        public string Climate { get; set; }

        [StringLength(50)]
        public string Terrain { get; set; }

        // Navigation properties
        public ICollection<Destination> Destinations { get; set; }
        public ICollection<CityCommonActivity> CityCommonActivities { get; set; }
        // Add other navigation properties as needed
    }
}
