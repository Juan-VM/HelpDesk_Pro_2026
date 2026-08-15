using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace HelpDesk_Pro_2026.Models
{
    [Table("priorities")]
    public class Priority : BaseModel
    {
        [PrimaryKey("priority_id", true)]
        public int PriorityId { get; set; }

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("color")]
        public string Color { get; set; } = string.Empty;
    }
}