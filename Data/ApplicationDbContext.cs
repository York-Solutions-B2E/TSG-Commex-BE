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
    public DbSet<Communication> Communications { get; set; }
    public DbSet<CommunicationType> CommunicationTypes { get; set; }
    public DbSet<CommunicationStatusHistory> CommunicationStatusHistories { get; set; }
    public DbSet<GlobalStatus> GlobalStatuses { get; set; }
    public DbSet<StatusTransition> StatusTransitions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // CommunicationType configuration
        modelBuilder.Entity<CommunicationType>()
            .HasKey(ct => ct.TypeCode);

        // GlobalStatus configuration
        modelBuilder.Entity<GlobalStatus>()
            .HasKey(gs => gs.StatusCode);

        modelBuilder.Entity<GlobalStatus>()
            .Property(gs => gs.Phase)
            .HasConversion<string>();

        // StatusTransition configuration
        modelBuilder.Entity<StatusTransition>()
            .HasKey(st => st.Id);

        // Communication relationships
        modelBuilder.Entity<Communication>()
            .HasOne(c => c.Type)
            .WithMany(ct => ct.Communications)
            .HasForeignKey(c => c.TypeCode);

        modelBuilder.Entity<Communication>()
            .HasOne<GlobalStatus>()
            .WithMany(gs => gs.Communications)
            .HasForeignKey(c => c.CurrentStatus);

        // StatusTransition relationships
        modelBuilder.Entity<StatusTransition>()
            .HasOne(st => st.CommunicationType)
            .WithMany()
            .HasForeignKey(st => st.TypeCode);

        modelBuilder.Entity<StatusTransition>()
            .HasOne(st => st.FromStatus)
            .WithMany(gs => gs.FromTransitions)
            .HasForeignKey(st => st.FromStatusCode)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StatusTransition>()
            .HasOne(st => st.ToStatus)
            .WithMany(gs => gs.ToTransitions)
            .HasForeignKey(st => st.ToStatusCode)
            .OnDelete(DeleteBehavior.Restrict);

        // StatusHistory relationships
        modelBuilder.Entity<CommunicationStatusHistory>()
            .HasOne(csh => csh.Communication)
            .WithMany(c => c.StatusHistory)
            .HasForeignKey(csh => csh.CommunicationId);

        modelBuilder.Entity<CommunicationStatusHistory>()
            .HasOne<GlobalStatus>()
            .WithMany(gs => gs.StatusHistories)
            .HasForeignKey(csh => csh.StatusCode);

        modelBuilder.Entity<CommunicationStatusHistory>()
            .HasOne<StatusTransition>()
            .WithMany(st => st.StatusHistories)
            .HasForeignKey(csh => csh.TransitionId)
            .OnDelete(DeleteBehavior.SetNull);

        // Constraints
        modelBuilder.Entity<StatusTransition>()
            .HasIndex(st => new { st.TypeCode, st.FromStatusCode, st.ToStatusCode })
            .IsUnique();

        // Prevent self-referencing transitions
        modelBuilder.Entity<StatusTransition>()
            .ToTable(t => t.HasCheckConstraint("CK_NoSelfReference", "[FromStatusCode] != [ToStatusCode]"));
    }
}