using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelApp.Models.Entities
{
    [Index(nameof(Email), IsUnique = true)]
    public class User
    {
        [Key]
        public Guid ID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }


        [Required]
        [StringLength(255, MinimumLength = 8)]

        public string? Password { get; set; }


        [Required]
        [ValidateNever]
        public bool IsAdmin { get; set; } = false;


        public ICollection<Review> Review_History { get; set; } = new List<Review>();

        public ICollection<Destination> Visited { get; set; } = new List<Destination>();
        public ICollection<Destination> Favorited { get; set; } = new List<Destination>();

        public ICollection<Preference> Preferences { get; set; } = new List<Preference>();

        public ICollection<Accommodation> Booking_History { get; set; } = new List<Accommodation>();
    }

    public class Visit
    {
        [Required]
        public Guid UserID { get; set; }

        [Required]
        public Guid DestinationID { get; set; }

        [Required]
        public DateTime Visit_Date { get; set; } = DateTime.Now; 

    }

    [Index(nameof(Content), IsUnique = true)]
    public class Preference
    {
        [Key]
        public Guid ID { get; set; }
        [Required]
        [MaxLength(100)]
        public string Content { get; set; }
    }

}
