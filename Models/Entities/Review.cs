// Models/Entities/Review.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TravelApp.Models.Entities
{
    public class Review : IValidatableObject
    {
        [Key]
        public Guid ID { get; set; }

        [Required(ErrorMessage = "Rating is required.")]
        [Range(0, 5, ErrorMessage = "Invalid rating (Must be between 0 and 5).")]
        public int? Rating { get; set; }

        [Required(ErrorMessage = "User is required.")]
        public Guid UserID { get; set; } // Foreign Key to User

        [ForeignKey("UserID")]
        [ValidateNever]
        public User User { get; set; }

        [MaxLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters.")]
        public string? Comment { get; set; }

        // Foreign Keys for one-to-many relationships
        public Guid? DestinationID { get; set; }

        [ForeignKey("DestinationID")]
        [ValidateNever]
        public Destination? Destination { get; set; }

        public Guid? AccommodationID { get; set; }

        [ForeignKey("AccommodationID")]
        [ValidateNever]
        public Accommodation? Accommodation { get; set; }

        public Guid? ActivityID { get; set; }

        [ForeignKey("ActivityID")]
        [ValidateNever]
        public Activity? Activity { get; set; }

        // Custom Validation to ensure only one entity association
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            int associationCount = 0;

            if (DestinationID.HasValue)
                associationCount++;
            if (AccommodationID.HasValue)
                associationCount++;
            if (ActivityID.HasValue)
                associationCount++;

            if (associationCount != 1)
            {
                yield return new ValidationResult(
                    "A review must be associated with exactly one entity: Destination, Accommodation, or Activity.",
                    new[] { "DestinationID", "AccommodationID", "ActivityID" }
                );
            }
        }
    }
}
