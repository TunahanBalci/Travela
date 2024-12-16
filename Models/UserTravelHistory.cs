using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TravelApp.Models;

namespace TravelApp.Models
{
    public class UserTravelHistory
    {
        [ForeignKey("User")]
        public int User_ID { get; set; }

        [ForeignKey("Destination")]
        public int Destination_ID { get; set; }

        public DateTime Visit_Date { get; set; }

        // Navigation properties
        public User User { get; set; }
        public Destination Destination { get; set; }
    }
}
