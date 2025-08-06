using Microsoft.EntityFrameworkCore;
using TSG_Commex_BE.Models.Domain;

namespace TSG_Commex_BE.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets - one for each table
    public DbSet<User> Users { get; set; }
    public DbSet<Member> Members { get; set; }
    public DbSet<Communication> Communications { get; set; }
    public DbSet<CommunicationType> CommunicationTypes { get; set; }
    public DbSet<CommunicationStatusHistory> CommunicationStatusHistories { get; set; }
    public DbSet<GlobalStatus> GlobalStatuses { get; set; }
    public DbSet<CommunicationTypeStatus> CommunicationTypeStatuses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User configuration
        modelBuilder.Entity<User>()
            .HasKey(u => u.Id);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Member configuration
        modelBuilder.Entity<Member>()
            .HasKey(m => m.Id);
        
        modelBuilder.Entity<Member>()
            .HasIndex(m => m.MemberId)
            .IsUnique();
        
        modelBuilder.Entity<Member>()
            .HasIndex(m => m.Email)
            .IsUnique();

        // CommunicationType configuration
        modelBuilder.Entity<CommunicationType>()
            .HasKey(ct => ct.Id);
        
        modelBuilder.Entity<CommunicationType>()
            .HasIndex(ct => ct.TypeCode)
            .IsUnique();

        // GlobalStatus configuration
        modelBuilder.Entity<GlobalStatus>()
            .HasKey(gs => gs.Id);
        
        modelBuilder.Entity<GlobalStatus>()
            .HasIndex(gs => gs.StatusCode)
            .IsUnique();

        modelBuilder.Entity<GlobalStatus>()
            .Property(gs => gs.Phase)
            .HasConversion<string>();

        // CommunicationTypeStatus configuration
        modelBuilder.Entity<CommunicationTypeStatus>()
            .HasKey(cts => cts.Id);

        // Communication relationships
        modelBuilder.Entity<Communication>()
            .HasOne(c => c.CommunicationType)
            .WithMany(ct => ct.Communications)
            .HasForeignKey(c => c.CommunicationTypeId);

        modelBuilder.Entity<Communication>()
            .HasOne(c => c.CurrentStatus)
            .WithMany(gs => gs.Communications)
            .HasForeignKey(c => c.CurrentStatusId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<Communication>()
            .HasOne(c => c.Member)
            .WithMany(m => m.Communications)
            .HasForeignKey(c => c.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        // Communication User relationships
        modelBuilder.Entity<Communication>()
            .HasOne(c => c.CreatedByUser)
            .WithMany(u => u.CreatedCommunications)
            .HasForeignKey(c => c.CreatedByUserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Communication>()
            .HasOne(c => c.LastUpdatedByUser)
            .WithMany(u => u.LastUpdatedCommunications)
            .HasForeignKey(c => c.LastUpdatedByUserId)
            .OnDelete(DeleteBehavior.NoAction);

        // CommunicationTypeStatus relationships
        modelBuilder.Entity<CommunicationTypeStatus>()
            .HasOne(cts => cts.CommunicationType)
            .WithMany()
            .HasForeignKey(cts => cts.CommunicationTypeId);

        modelBuilder.Entity<CommunicationTypeStatus>()
            .HasOne(cts => cts.GlobalStatus)
            .WithMany()
            .HasForeignKey(cts => cts.GlobalStatusId);

        // StatusHistory relationships
        modelBuilder.Entity<CommunicationStatusHistory>()
            .HasOne(csh => csh.Communication)
            .WithMany(c => c.StatusHistory)
            .HasForeignKey(csh => csh.CommunicationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CommunicationStatusHistory>()
            .HasOne(csh => csh.GlobalStatus)
            .WithMany(gs => gs.StatusHistories)
            .HasForeignKey(csh => csh.GlobalStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CommunicationStatusHistory>()
            .HasOne(csh => csh.UpdatedByUser)
            .WithMany(u => u.StatusHistoryEntries)
            .HasForeignKey(csh => csh.UpdatedByUserId)
            .OnDelete(DeleteBehavior.NoAction);

        // Note: Global Query Filters removed to avoid relationship warnings
        // Soft delete filtering will be handled manually in repositories
    }
}