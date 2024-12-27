using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TravelApp.Models.ViewModels
{
    public class CreateUserViewModel
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(255, MinimumLength = 8)]
        public string Password { get; set; }

        public List<SelectListItem> Preferences { get; set; } = new List<SelectListItem>();

        public List<Guid> SelectedPreferenceIds { get; set; } = new List<Guid>();
    }
}
