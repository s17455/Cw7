using System.ComponentModel.DataAnnotations;

namespace Cw7.DTOs.Responses
{
    public class LoginResponse
    {
        [Required]
        public string Token { get; set; }
        [Required]
        public string RefreshToken { get; set; }
    }
}
