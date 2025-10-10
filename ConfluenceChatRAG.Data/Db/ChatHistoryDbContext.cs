using Microsoft.EntityFrameworkCore;

namespace ConfluenceChatRAG.Data.Db;

public class ChatHistoryDbContext(DbContextOptions<ChatHistoryDbContext> options)
    : DbContext(options)
{
    public DbSet<ChatMessageEntity> ChatMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChatMessageEntity>(entity =>
        {
            entity.HasIndex(e => e.SessionId);
            entity.HasIndex(e => e.Timestamp);
        });
    }
}