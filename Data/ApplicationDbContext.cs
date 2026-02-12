using Microsoft.EntityFrameworkCore;
using DMSMigration.Core.Entities;

namespace DMSMigration.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<DmsDocument> DmsDocuments { get; set; }
    public DbSet<DmsDocumentIndex> DmsDocumentIndexes { get; set; }
    public DbSet<DmsDocumentVersion> DmsDocumentVersions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // DMS_DOCUMENT configuration
        modelBuilder.Entity<DmsDocument>(entity =>
        {
            entity.ToTable("DMS_DOCUMENT");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("ID").ValueGeneratedOnAdd();
            entity.Property(e => e.FileName).HasColumnName("FILE_NAME").HasMaxLength(500).IsRequired();
            entity.Property(e => e.Extension).HasColumnName("EXTENSION").HasMaxLength(500).IsRequired();
            entity.Property(e => e.Path).HasColumnName("PATH").HasMaxLength(500).IsRequired();
            entity.Property(e => e.Size).HasColumnName("SIZE").IsRequired();
            entity.Property(e => e.TypeId).HasColumnName("TYPE_ID").IsRequired();
            entity.Property(e => e.CurrentVersion).HasColumnName("CURRENT_VERSION").IsRequired();
            entity.Property(e => e.CreationTime).HasColumnName("CREATION_TIME").HasPrecision(7).IsRequired();
            entity.Property(e => e.CreatorUserId).HasColumnName("CREATOR_USER_ID");
            entity.Property(e => e.LastModificationTime).HasColumnName("LAST_MODIFICATION_TIME").HasPrecision(7);
            entity.Property(e => e.LastModifierUserId).HasColumnName("LAST_MODIFIER_USER_ID");
            entity.Property(e => e.IsDeleted).HasColumnName("IS_DELETED").IsRequired();
            entity.Property(e => e.DeleterUserId).HasColumnName("DELETER_USER_ID");
            entity.Property(e => e.DeletionTime).HasColumnName("DELETION_TIME").HasPrecision(7);
            entity.Property(e => e.TenantId).HasColumnName("TENANT_ID");

            // Index on FileName
            entity.HasIndex(e => e.FileName).HasDatabaseName("IDX_DMS_DOCUMENT_FILENAME");

            // Relationships
            entity.HasMany(e => e.Indexes)
                .WithOne(e => e.Document)
                .HasForeignKey(e => e.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Versions)
                .WithOne(e => e.Document)
                .HasForeignKey(e => e.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // DMS_DOCUMENT_INDEX configuration
        modelBuilder.Entity<DmsDocumentIndex>(entity =>
        {
            entity.ToTable("DMS_DOCUMENT_INDEX");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("ID").ValueGeneratedOnAdd();
            entity.Property(e => e.DocumentId).HasColumnName("DOCUMENT_ID").IsRequired();
            entity.Property(e => e.IndexKey).HasColumnName("INDEX_KEY").HasMaxLength(100).IsRequired();
            entity.Property(e => e.IndexValue).HasColumnName("INDEX_VALUE").HasMaxLength(500).IsRequired();
            entity.Property(e => e.CreationTime).HasColumnName("CREATION_TIME").HasPrecision(7).IsRequired();

            // Composite index on DocumentId and IndexKey
            entity.HasIndex(e => new { e.DocumentId, e.IndexKey }).HasDatabaseName("IDX_DMS_DOCINDEX_DOCID_KEY");
        });

        // DMS_DOCUMENT_VERSION configuration
        modelBuilder.Entity<DmsDocumentVersion>(entity =>
        {
            entity.ToTable("DMS_DOCUMENT_VERSION");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("ID").ValueGeneratedOnAdd();
            entity.Property(e => e.DocumentId).HasColumnName("DOCUMENT_ID").IsRequired();
            entity.Property(e => e.VersionNumber).HasColumnName("VERSION_NUMBER").IsRequired();
            entity.Property(e => e.FileName).HasColumnName("FILE_NAME").HasMaxLength(500).IsRequired();
            entity.Property(e => e.Path).HasColumnName("PATH").HasMaxLength(500).IsRequired();
            entity.Property(e => e.Size).HasColumnName("SIZE").IsRequired();
            entity.Property(e => e.CreationTime).HasColumnName("CREATION_TIME").HasPrecision(7).IsRequired();
            entity.Property(e => e.CreatorUserId).HasColumnName("CREATOR_USER_ID");
        });
    }
}
