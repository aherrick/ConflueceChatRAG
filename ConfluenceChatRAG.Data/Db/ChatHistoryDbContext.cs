using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ConfluenceChatRAG.Data.Models.Dto;

namespace ConfluenceChatRAG.Data.Db;

public class ChatHistoryDbContext(DbContextOptions<ChatHistoryDbContext> options)
    : DbContext(options)
{
    public DbSet<ChatMessageEntity> ChatMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var sourcesConverter = new ValueConverter<List<ChatSourceDto>, string>(
            v => v == null ? null : System.Text.Json.JsonSerializer.Serialize(v),
            v => string.IsNullOrEmpty(v) ? new List<ChatSourceDto>() : System.Text.Json.JsonSerializer.Deserialize<List<ChatSourceDto>>(v) ?? new List<ChatSourceDto>()
        );
        var suggestionsConverter = new ValueConverter<List<string>, string>(
            v => v == null ? null : System.Text.Json.JsonSerializer.Serialize(v),
            v => string.IsNullOrEmpty(v) ? new List<string>() : System.Text.Json.JsonSerializer.Deserialize<List<string>>(v) ?? new List<string>()
        );

        modelBuilder.Entity<ChatMessageEntity>(entity =>
        {
            entity.HasIndex(e => e.SessionId);
            entity.HasIndex(e => e.Timestamp);
            entity.Property(e => e.SessionId)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.IsUser)
                .IsRequired();
            entity.Property(e => e.Content)
                .IsRequired();
            entity.Property(e => e.Sources)
                .HasConversion(sourcesConverter)
                .HasColumnName("SourcesJson");
            entity.Property(e => e.Suggestions)
                .HasConversion(suggestionsConverter)
                .HasColumnName("SuggestionsJson");
        });
    }
}