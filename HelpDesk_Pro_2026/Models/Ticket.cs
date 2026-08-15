using Newtonsoft.Json;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace HelpDesk_Pro_2026.Models
{
    [Table("tickets")]
    public class Ticket : BaseModel
    {
        [PrimaryKey("ticket_id", false)]
        public long TicketId { get; set; }

        [Column("ticket_code")]
        public string TicketCode { get; set; } = string.Empty;

        [Column("system_id")]
        public int SystemId { get; set; }

        [Column("category_id")]
        public int CategoryId { get; set; }

        [Column("priority_id")]
        public int PriorityId { get; set; }

        [Column("risk_id")]
        public int RiskId { get; set; }

        [Column("state_id")]
        public int StateId { get; set; }

        [Column("subject")]
        public string Subject { get; set; } = string.Empty;

        [Column("description")]
        public string Description { get; set; } = string.Empty;

        [Column("justification")]
        public string? Justification { get; set; }

        [Column("requester_id")]
        public Guid RequesterId { get; set; }

        [Column("assigned_to")]
        public Guid AssignedTo { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [Column("attachment_url")]
        public string? AttachmentUrl { get; set; }

        // --- NAVIGATION RELATIONSHIPS ---

        [Reference(typeof(SystemCatalog))]
        public SystemCatalog? System { get; set; }

        [Reference(typeof(Category))]
        public Category? Category { get; set; }

        [Reference(typeof(Priority))]
        public Priority? Priority { get; set; }

        [Reference(typeof(RiskLevel))]
        public RiskLevel? Risk { get; set; }

        [Reference(typeof(TicketState))]
        public TicketState? State { get; set; }

        [Reference(typeof(Usuario), foreignKey: "tickets_requester_id_fkey")]
        public Usuario? Requester { get; set; }

        [Reference(typeof(Usuario), foreignKey: "tickets_assigned_to_fkey")]
        public Usuario? AssignedTech { get; set; }

        // Tells Newtonsoft.Json (which Supabase uses) to ignore this property in SQL queries/inserts
        [JsonIgnore]
        public List<TicketAttachment> Attachments { get; set; } = new();
    }
}