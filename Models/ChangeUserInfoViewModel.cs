using System.ComponentModel.DataAnnotations;

namespace WarungElja.Models
{
    public class ChangeUserInfoViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Current Password")]
        public string? CurrentPassword { get; set; }

        [Display(Name = "New Password")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string? NewPassword { get; set; }

        [Display(Name = "Confirm New Password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "Role")]
        public string Role { get; set; } = string.Empty;
    }
}