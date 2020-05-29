using System.ComponentModel.DataAnnotations;

namespace Cw7.DTOs.Responses
{
    public class ErrorResponse
    {
        [Required]
        public string Message { get; set; }
    }
}
