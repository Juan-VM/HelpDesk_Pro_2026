using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace HelpDesk_Pro_2026.Models
{
    [Table("categories")]
    public class Category : BaseModel
    {
        [PrimaryKey("category_id", true)]
        public int CategoryId { get; set; }

        [Column("name")]
        public string Name { get; set; } = string.Empty;
    }
}