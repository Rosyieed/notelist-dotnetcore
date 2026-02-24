using System.ComponentModel.DataAnnotations;

namespace NoteList.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(100, ErrorMessage = "Username cannot be longer than 100 characters.")]
        public string Username { get; set; } = "";

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [StringLength(150, ErrorMessage = "Email cannot be longer than 150 characters.")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Password is required")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Confirm Password is required")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password and Confirm Password do not match.")]
        public string ConfirmPassword { get; set; } = "";
    }
}
