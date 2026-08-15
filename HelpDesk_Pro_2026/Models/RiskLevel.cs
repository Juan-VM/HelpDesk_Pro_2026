using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace HelpDesk_Pro_2026.Models
{
    [Table("risk_levels")]
    public class RiskLevel : BaseModel
    {
        [PrimaryKey("risk_id", true)]
        public int RiskId { get; set; }

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("color")]
        public string Color { get; set; } = string.Empty;
    }
}