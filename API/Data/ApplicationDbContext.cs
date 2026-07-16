using System.Text.Json;
using API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace API.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<CustomerForm> CustomerForms => Set<CustomerForm>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<ProjectDocument> ProjectDocuments => Set<ProjectDocument>();
    public DbSet<DocumentVersion> DocumentVersions => Set<DocumentVersion>();
    public DbSet<LookupItem> LookupItems => Set<LookupItem>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(u => u.LastName).HasMaxLength(100).IsRequired();
            entity.Ignore(u => u.FullName);
        });

        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Token).HasMaxLength(512).IsRequired();
            entity.HasIndex(r => r.Token).IsUnique();
            entity.HasOne(r => r.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Ignore(r => r.IsExpired);
            entity.Ignore(r => r.IsActive);
        });

        builder.Entity<Client>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.CompanyName).HasMaxLength(200).IsRequired();
            entity.Property(c => c.Industry).HasMaxLength(200);
            entity.Property(c => c.PrimaryContact).HasMaxLength(200);
            entity.Property(c => c.Email).HasMaxLength(256);
            entity.Property(c => c.Phone).HasMaxLength(50);
            entity.Property(c => c.Country).HasMaxLength(100);
            entity.HasIndex(c => c.CompanyName);
            entity.HasMany(c => c.Projects)
                .WithOne(p => p.Client)
                .HasForeignKey(p => p.ClientId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Project>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).HasMaxLength(200).IsRequired();
            entity.Property(p => p.ClientName).HasMaxLength(200).IsRequired();
            entity.Property(p => p.ApplicationName).HasMaxLength(200).IsRequired();
            entity.Property(p => p.Priority).HasMaxLength(50);
            entity.Property(p => p.FormToken).HasMaxLength(128);
            entity.Property(p => p.FormLink).HasMaxLength(500);
            entity.Property(p => p.CreatedBy).HasMaxLength(256).IsRequired();
            entity.Property(p => p.Status).HasConversion<string>().HasMaxLength(32);
            entity.HasIndex(p => p.FormToken).IsUnique().HasFilter("[FormToken] IS NOT NULL");
            entity.HasIndex(p => p.ClientName);
            entity.HasOne(p => p.ProjectDocument)
                .WithOne(d => d.Project)
                .HasForeignKey<ProjectDocument>(d => d.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        var formDataConverter = new ValueConverter<Dictionary<string, string>, string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null)
                 ?? new Dictionary<string, string>());

        var formDataComparer = new ValueComparer<Dictionary<string, string>>(
            (a, b) => JsonSerializer.Serialize(a, (JsonSerializerOptions?)null) == JsonSerializer.Serialize(b, (JsonSerializerOptions?)null),
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null).GetHashCode(),
            v => JsonSerializer.Deserialize<Dictionary<string, string>>(
                     JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                     (JsonSerializerOptions?)null)
                 ?? new Dictionary<string, string>());

        builder.Entity<CustomerForm>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.Property(f => f.Token).HasMaxLength(128).IsRequired();
            entity.Property(f => f.FormData)
                .HasConversion(formDataConverter)
                .HasColumnType("nvarchar(max)")
                .Metadata.SetValueComparer(formDataComparer);
            entity.HasIndex(f => f.Token).IsUnique();
            entity.HasOne(f => f.Project)
                .WithMany(p => p.CustomerForms)
                .HasForeignKey(f => f.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Attachment>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.FileName).HasMaxLength(500).IsRequired();
            entity.Property(a => a.ContentType).HasMaxLength(200);
            entity.Property(a => a.StoragePath).HasMaxLength(1000);
            entity.Property(a => a.UploadedBy).HasMaxLength(256);
            entity.HasOne(a => a.Project)
                .WithMany(p => p.Attachments)
                .HasForeignKey(a => a.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ProjectDocument>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.HasIndex(d => d.ProjectId).IsUnique();
            entity.HasMany(d => d.Versions)
                .WithOne(v => v.ProjectDocument)
                .HasForeignKey(v => v.ProjectDocumentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<DocumentVersion>(entity =>
        {
            entity.HasKey(v => v.Id);
            entity.Property(v => v.SaveType).HasMaxLength(50).IsRequired();
            entity.Property(v => v.FormDataJson).HasColumnType("nvarchar(max)");
            entity.Property(v => v.AttachmentsJson).HasColumnType("nvarchar(max)");
            entity.Property(v => v.GeneratedDocumentsJson).HasColumnType("nvarchar(max)");
            entity.HasIndex(v => new { v.ProjectDocumentId, v.VersionNumber }).IsUnique();
        });

        builder.Entity<LookupItem>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.Category).HasMaxLength(100).IsRequired();
            entity.Property(l => l.Code).HasMaxLength(100).IsRequired();
            entity.Property(l => l.DisplayName).HasMaxLength(200).IsRequired();
            entity.HasIndex(l => new { l.Category, l.Code }).IsUnique();
        });

        SeedLookups(builder);
    }

    private static void SeedLookups(ModelBuilder builder)
    {
        builder.Entity<LookupItem>().HasData(
            new LookupItem { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0001"), Category = "ProjectStatus", Code = "Draft", DisplayName = "Draft", SortOrder = 1 },
            new LookupItem { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0002"), Category = "ProjectStatus", Code = "InProgress", DisplayName = "In Progress", SortOrder = 2 },
            new LookupItem { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0003"), Category = "ProjectStatus", Code = "PendingReview", DisplayName = "Pending Review", SortOrder = 3 },
            new LookupItem { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0004"), Category = "ProjectStatus", Code = "Completed", DisplayName = "Completed", SortOrder = 4 },
            new LookupItem { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0005"), Category = "Priority", Code = "Low", DisplayName = "Low", SortOrder = 1 },
            new LookupItem { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0006"), Category = "Priority", Code = "Medium", DisplayName = "Medium", SortOrder = 2 },
            new LookupItem { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0007"), Category = "Priority", Code = "High", DisplayName = "High", SortOrder = 3 },
            new LookupItem { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0008"), Category = "ApplicationType", Code = "LDAP", DisplayName = "LDAP", SortOrder = 1 },
            new LookupItem { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0009"), Category = "ApplicationType", Code = "DB", DisplayName = "DB Based", SortOrder = 2 },
            new LookupItem { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0010"), Category = "ApplicationType", Code = "API", DisplayName = "API Based", SortOrder = 3 },
            new LookupItem { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0011"), Category = "ApplicationType", Code = "SDK", DisplayName = "SDK Based", SortOrder = 4 },
            new LookupItem { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0012"), Category = "ApplicationType", Code = "RPA", DisplayName = "RPA Based", SortOrder = 5 },
            new LookupItem { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0013"), Category = "Role", Code = "Admin", DisplayName = "Admin", SortOrder = 1 },
            new LookupItem { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0014"), Category = "Role", Code = "Engineer", DisplayName = "Implementation Engineer", SortOrder = 2 },
            new LookupItem { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaa0015"), Category = "Role", Code = "Viewer", DisplayName = "Viewer", SortOrder = 3 }
        );
    }
}
