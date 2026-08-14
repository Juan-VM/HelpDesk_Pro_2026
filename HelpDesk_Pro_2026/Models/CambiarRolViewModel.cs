using System.ComponentModel.DataAnnotations;

namespace HelpDesk_Pro_2026.Models
{
    public class CambiarRolViewModel
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public string Role { get; set; } = string.Empty;
    }
}