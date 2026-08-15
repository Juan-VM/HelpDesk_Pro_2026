using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace HelpDesk_Pro_2026.Models
{
    [Table("ticket_attachments")]
    public class TicketAttachment : BaseModel
    {
        [PrimaryKey("attachment_id", false)]
        public long AttachmentId { get; set; }

        [Column("ticket_id")]
        public long TicketId { get; set; }

        [Column("file_name")]
        public string FileName { get; set; } = string.Empty;

        [Column("file_path")]
        public string FilePath { get; set; } = string.Empty;

        [Column("file_url")]
        public string FileUrl { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}