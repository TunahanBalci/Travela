using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelApp.Models
{
    public class Booking
    {
        [Key]
        public int Booking_ID { get; set; }

        [ForeignKey("User")]
        public int User_ID { get; set; }

        [ForeignKey("Accommodation")]
        public int Accommodation_ID { get; set; }

        public DateTime Start_Date { get; set; }
        public DateTime End_Date { get; set; }
        public DateTime Booking_Date { get; set; }

        // Navigation properties
        public User User { get; set; }
        public Accommodation Accommodation { get; set; }
    }
}
