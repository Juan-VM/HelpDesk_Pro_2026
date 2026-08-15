using Newtonsoft.Json;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace HelpDesk_Pro_2026.Models
{
    [Table("comments")]
    public class Comment : BaseModel
    {
        [PrimaryKey("comment_id", false)]
        public long CommentId { get; set; }

        [Column("ticket_id")]
        public long TicketId { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("comment")]
        public string CommentText { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        // Populated manually in TicketService to avoid Postgrest embedding conflicts
        [JsonIgnore]
        public Usuario? User { get; set; }
    }
}