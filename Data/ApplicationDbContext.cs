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
    public DbSet<DmsIndex> DmsIndexes { get; set; }
    public DbSet<DmsDocumentType> DmsDocumentTypes { get; set; }

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
            entity.Property(e => e.Extension).HasColumnName("EXTENSION").HasMaxLength(500);
            entity.Property(e => e.Path).HasColumnName("PATH").HasMaxLength(500);
            entity.Property(e => e.Size).HasColumnName("SIZE");
            entity.Property(e => e.TypeId).HasColumnName("TYPE_ID");
            entity.Property(e => e.CurrentVersion).HasColumnName("CURRENT_VERSION");
            entity.Property(e => e.CreationTime).HasColumnName("CREATION_TIME").HasPrecision(7);
            entity.Property(e => e.CreatorUserId).HasColumnName("CREATOR_USER_ID");
            entity.Property(e => e.LastModificationTime).HasColumnName("LAST_MODIFICATION_TIME").HasPrecision(7);
            entity.Property(e => e.LastModifierUserId).HasColumnName("LAST_MODIFIER_USER_ID");
            entity.Property(e => e.IsDeleted).HasColumnName("IS_DELETED");
            entity.Property(e => e.DeleterUserId).HasColumnName("DELETER_USER_ID");
            entity.Property(e => e.DeletionTime).HasColumnName("DELETION_TIME").HasPrecision(7);
            entity.Property(e => e.TenantId).HasColumnName("TENANT_ID");

            entity.HasIndex(e => e.FileName).HasDatabaseName("IDX_DMS_DOCUMENT_FILENAME");

            entity.HasMany(e => e.Indexes)
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
            entity.Property(e => e.Value).HasColumnName("VALUE").HasMaxLength(300);
            entity.Property(e => e.DocumentId).HasColumnName("DOCUMENT_ID");
            entity.Property(e => e.IndexId).HasColumnName("INDEX_ID");
            entity.Property(e => e.CreationTime).HasColumnName("CREATION_TIME").HasPrecision(7);
            entity.Property(e => e.CreatorUserId).HasColumnName("CREATOR_USER_ID");
            entity.Property(e => e.LastModificationTime).HasColumnName("LAST_MODIFICATION_TIME").HasPrecision(7);
            entity.Property(e => e.LastModifierUserId).HasColumnName("LAST_MODIFIER_USER_ID");
            entity.Property(e => e.IsDeleted).HasColumnName("IS_DELETED");
            entity.Property(e => e.DeleterUserId).HasColumnName("DELETER_USER_ID");
            entity.Property(e => e.DeletionTime).HasColumnName("DELETION_TIME").HasPrecision(7);
            entity.Property(e => e.TenantId).HasColumnName("TENANT_ID");

            entity.HasOne(e => e.Document)
                .WithMany(e => e.Indexes)
                .HasForeignKey(e => e.DocumentId);

            entity.HasOne(e => e.Index)
                .WithMany(e => e.DocumentIndexes)
                .HasForeignKey(e => e.IndexId);
        });

        // DMS_INDEX configuration
        modelBuilder.Entity<DmsIndex>(entity =>
        {
            entity.ToTable("DMS_INDEX");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("ID").ValueGeneratedOnAdd();
            entity.Property(e => e.Key).HasColumnName("KEY").HasMaxLength(200);
            entity.Property(e => e.CreationTime).HasColumnName("CREATION_TIME").HasPrecision(7);
            entity.Property(e => e.CreatorUserId).HasColumnName("CREATOR_USER_ID");
            entity.Property(e => e.LastModificationTime).HasColumnName("LAST_MODIFICATION_TIME").HasPrecision(7);
            entity.Property(e => e.LastModifierUserId).HasColumnName("LAST_MODIFIER_USER_ID");
            entity.Property(e => e.IsDeleted).HasColumnName("IS_DELETED");
            entity.Property(e => e.DeleterUserId).HasColumnName("DELETER_USER_ID");
            entity.Property(e => e.DeletionTime).HasColumnName("DELETION_TIME").HasPrecision(7);
            entity.Property(e => e.TenantId).HasColumnName("TENANT_ID");

            entity.HasIndex(e => e.Key).HasDatabaseName("IDX_DMS_INDEX_KEY");
        });

        // DMS_DOCUMENT_TYPE configuration
        modelBuilder.Entity<DmsDocumentType>(entity =>
        {
            entity.ToTable("DMS_DOCUMENT_TYPE");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("ID").ValueGeneratedOnAdd();
            entity.Property(e => e.Key).HasColumnName("KEY").HasMaxLength(200);
            entity.Property(e => e.Name).HasColumnName("NAME").HasMaxLength(2000);
            entity.Property(e => e.IsSystem).HasColumnName("IS_SYSTEM");
            entity.Property(e => e.HasMultipleDocument).HasColumnName("HAS_MULTIPLE_DOCUMENT");
            entity.Property(e => e.ParentId).HasColumnName("PARENT_ID");
            entity.Property(e => e.CreationTime).HasColumnName("CREATION_TIME").HasPrecision(7);
            entity.Property(e => e.CreatorUserId).HasColumnName("CREATOR_USER_ID");
            entity.Property(e => e.LastModificationTime).HasColumnName("LAST_MODIFICATION_TIME").HasPrecision(7);
            entity.Property(e => e.LastModifierUserId).HasColumnName("LAST_MODIFIER_USER_ID");
            entity.Property(e => e.IsDeleted).HasColumnName("IS_DELETED");
            entity.Property(e => e.DeleterUserId).HasColumnName("DELETER_USER_ID");
            entity.Property(e => e.DeletionTime).HasColumnName("DELETION_TIME").HasPrecision(7);
            entity.Property(e => e.TenantId).HasColumnName("TENANT_ID");
            entity.Property(e => e.IsDeletable).HasColumnName("IS_DELETABLE").HasDefaultValue(0);
        });
    }
}
