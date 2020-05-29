using System.ComponentModel.DataAnnotations;

namespace Cw7.Models
{
    public class Studies
    {
        [Required]
        public int IdStudy { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
