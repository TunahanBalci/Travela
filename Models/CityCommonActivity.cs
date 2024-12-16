using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TravelApp.Models;

namespace TravelApp.Models
{
    public class CityCommonActivity
    {
        [ForeignKey("City")]
        public int City_ID { get; set; }

        [Key]
        [StringLength(100)]
        public string Activity { get; set; }

        // Navigation properties
        public City City { get; set; }
    }
}
