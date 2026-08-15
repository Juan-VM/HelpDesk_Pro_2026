using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace HelpDesk_Pro_2026.Models
{
    [Table("ticket_states")]
    public class TicketState : BaseModel
    {
        [PrimaryKey("state_id", true)]
        public int StateId { get; set; }

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("order_number")]
        public short OrderNumber { get; set; }
    }
}