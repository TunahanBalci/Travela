using System.ComponentModel.DataAnnotations;

public class UpdatePasswordRequestViewModel
{
    [Required(ErrorMessage = "Current password is required.")]
    public string CurrentPassword { get; set; }

    [Required(ErrorMessage = "New password is required.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
    public string NewPassword { get; set; }

    [Required(ErrorMessage = "Confirm password is required.")]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; }
}
