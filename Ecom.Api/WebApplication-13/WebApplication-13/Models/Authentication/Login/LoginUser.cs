using System.ComponentModel.DataAnnotations;

namespace WebApplication_13.Models.Authentication.Login
{
    public class LoginUser
    {
        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }
    }
}
