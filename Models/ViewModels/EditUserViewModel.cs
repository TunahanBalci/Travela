using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TravelApp.Models.ViewModels
{
    public class EditUserViewModel
    {
        [Required]
        public Guid ID { get; set; }

        [Required(ErrorMessage = "User name is required.")]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(255, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
        public string Password { get; set; }

        [Display(Name = "Is Admin")]
        public bool IsAdmin { get; set; }


        public List<SelectListItem> Preferences { get; set; } = new List<SelectListItem>();
        public List<Guid> SelectedPreferenceIds { get; set; } = new List<Guid>();


        public List<SelectListItem> VisitedDestinations { get; set; } = new List<SelectListItem>();
        public List<Guid> SelectedVisitedDestinationIds { get; set; } = new List<Guid>();

        public List<SelectListItem> FavoriteDestinations { get; set; } = new List<SelectListItem>();
        public List<Guid> SelectedFavoriteDestinationIds { get; set; } = new List<Guid>();
    }
}
