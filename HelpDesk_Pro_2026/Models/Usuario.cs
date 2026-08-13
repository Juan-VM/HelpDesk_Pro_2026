
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace HelpDesk_Pro_2026.Models
{
    [Table("users")]
    public class Usuario : BaseModel
    {
        [PrimaryKey("user_id", false)]
        public Guid UserId { get; set; }

        [Column("full_name")]
        public string FullName { get; set; } = string.Empty;

        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Column("password")]
        public string Password { get; set; } = string.Empty;

        [Column("role")]
        public string Role { get; set; } = string.Empty;

        [Column("photo_url")]
        public string? PhotoUrl { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}