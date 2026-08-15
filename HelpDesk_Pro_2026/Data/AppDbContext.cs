using Microsoft.EntityFrameworkCore;

namespace HelpDesk_Pro_2026.Data
{
    // 1. Lightweight EF Core Entities (isolated from Supabase BaseModel)
    public class EfTicket
    {
        public long TicketId { get; set; }
        public int PriorityId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public EfPriority? Priority { get; set; }
    }

    public class EfPriority
    {
        public int PriorityId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }

    // 2. EF Core DbContext
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<EfTicket> Tickets { get; set; }
        public DbSet<EfPriority> Priorities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ticket Table Mapping
            modelBuilder.Entity<EfTicket>(entity =>
            {
                entity.ToTable("tickets");
                entity.HasKey(t => t.TicketId);
                entity.Property(t => t.TicketId).HasColumnName("ticket_id");
                entity.Property(t => t.PriorityId).HasColumnName("priority_id");
                entity.Property(t => t.Subject).HasColumnName("subject");
                entity.Property(t => t.CreatedAt).HasColumnName("created_at");

                entity.HasOne(t => t.Priority)
                      .WithMany()
                      .HasForeignKey(t => t.PriorityId);
            });

            // Priority Table Mapping
            modelBuilder.Entity<EfPriority>(entity =>
            {
                entity.ToTable("priorities");
                entity.HasKey(p => p.PriorityId);
                entity.Property(p => p.PriorityId).HasColumnName("priority_id");
                entity.Property(p => p.Name).HasColumnName("name");
                entity.Property(p => p.Color).HasColumnName("color");
            });
        }
    }
}