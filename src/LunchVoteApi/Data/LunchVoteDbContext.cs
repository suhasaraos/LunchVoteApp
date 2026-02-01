using LunchVoteApi.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace LunchVoteApi.Data;

/// <summary>
/// Entity Framework Core database context for the Lunch Vote application.
/// </summary>
public class LunchVoteDbContext : DbContext
{
    public LunchVoteDbContext(DbContextOptions<LunchVoteDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<Poll> Polls { get; set; } = null!;
    public DbSet<Option> Options { get; set; } = null!;
    public DbSet<Vote> Votes { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure Poll entity
        modelBuilder.Entity<Poll>(entity =>
        {
            entity.HasKey(p => p.Id);
            
            entity.Property(p => p.GroupId)
                .IsRequired()
                .HasMaxLength(50);
            
            entity.Property(p => p.Question)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(p => p.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
            
            entity.Property(p => p.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
            
            // Index for fast lookup by group
            entity.HasIndex(p => p.GroupId)
                .HasDatabaseName("IX_Poll_GroupId");
            
            // Index for active poll lookup
            entity.HasIndex(p => new { p.GroupId, p.IsActive })
                .HasDatabaseName("IX_Poll_GroupId_IsActive");
        });
        
        // Configure Option entity
        modelBuilder.Entity<Option>(entity =>
        {
            entity.HasKey(o => o.Id);
            
            entity.Property(o => o.Text)
                .IsRequired()
                .HasMaxLength(100);
            
            // Relationship: Poll -> Options (One-to-Many, Cascade Delete)
            entity.HasOne(o => o.Poll)
                .WithMany(p => p.Options)
                .HasForeignKey(o => o.PollId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Configure Vote entity
        modelBuilder.Entity<Vote>(entity =>
        {
            entity.HasKey(v => v.Id);
            
            entity.Property(v => v.VoterToken)
                .IsRequired()
                .HasMaxLength(64);
            
            entity.Property(v => v.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
            
            // Unique constraint to prevent duplicate votes
            entity.HasIndex(v => new { v.PollId, v.VoterToken })
                .IsUnique()
                .HasDatabaseName("UQ_Vote_Poll_VoterToken");
            
            // Relationship: Poll -> Votes (One-to-Many, Cascade Delete)
            entity.HasOne(v => v.Poll)
                .WithMany(p => p.Votes)
                .HasForeignKey(v => v.PollId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Relationship: Option -> Votes (One-to-Many, Restrict Delete)
            entity.HasOne(v => v.Option)
                .WithMany(o => o.Votes)
                .HasForeignKey(v => v.OptionId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
