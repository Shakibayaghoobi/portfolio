using Microsoft.EntityFrameworkCore;
using Portfolio.Domain;

namespace Portfolio.Infrastructure;

public class PortfolioDbContext(DbContextOptions<PortfolioDbContext> options) : DbContext(options)
{
    public DbSet<ContentEntry> ContentEntries => Set<ContentEntry>();
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();
    public DbSet<Section> Sections => Set<Section>();
    public DbSet<SeoMeta> SeoMetas => Set<SeoMeta>();
    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectImage> ProjectImages => Set<ProjectImage>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Award> Awards => Set<Award>();
    public DbSet<EducationItem> Education => Set<EducationItem>();
    public DbSet<ExperienceItem> Experience => Set<ExperienceItem>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<LanguageSkill> LanguageSkills => Set<LanguageSkill>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // Every LocalizedText property becomes two columns, e.g. Title_En / Title_Fa.
        foreach (var (type, prop) in new (Type, string)[]
        {
            (typeof(ContentEntry), nameof(ContentEntry.Value)), (typeof(SiteSetting), nameof(SiteSetting.Value)),
            (typeof(SeoMeta), nameof(SeoMeta.Title)), (typeof(SeoMeta), nameof(SeoMeta.Description)),
            (typeof(MediaAsset), nameof(MediaAsset.Alt)),
            (typeof(Project), nameof(Project.Title)), (typeof(Project), nameof(Project.Type)), (typeof(Project), nameof(Project.Note)),
            (typeof(Service), nameof(Service.Title)), (typeof(Service), nameof(Service.Text)),
            (typeof(Award), nameof(Award.Title)), (typeof(Award), nameof(Award.By)),
            (typeof(EducationItem), nameof(EducationItem.Title)), (typeof(EducationItem), nameof(EducationItem.Organization)),
            (typeof(ExperienceItem), nameof(ExperienceItem.Organization)), (typeof(ExperienceItem), nameof(ExperienceItem.Role)), (typeof(ExperienceItem), nameof(ExperienceItem.Note)),
            (typeof(LanguageSkill), nameof(LanguageSkill.Name)), (typeof(LanguageSkill), nameof(LanguageSkill.Level)),
        })
        {
            b.Entity(type).ComplexProperty(typeof(LocalizedText), prop, cp =>
            {
                cp.Property(nameof(LocalizedText.En)).HasColumnName($"{prop}_En");
                cp.Property(nameof(LocalizedText.Fa)).HasColumnName($"{prop}_Fa");
            });
        }

        b.Entity<ContentEntry>().HasIndex(x => x.Key).IsUnique();
        b.Entity<ContentEntry>().Property(x => x.Key).HasMaxLength(120);
        b.Entity<SiteSetting>().HasIndex(x => x.Key).IsUnique();
        b.Entity<SiteSetting>().Property(x => x.Key).HasMaxLength(120);
        b.Entity<Section>().HasIndex(x => x.Key).IsUnique();
        b.Entity<SeoMeta>().HasIndex(x => x.Route).IsUnique();
        b.Entity<Project>().HasIndex(x => x.Slug).IsUnique();
        b.Entity<AdminUser>().HasIndex(x => x.Email).IsUnique();
        b.Entity<MediaAsset>().Property(x => x.StoragePath).HasMaxLength(400);

        b.Entity<ProjectImage>().HasOne(x => x.Project).WithMany(p => p.Images).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<ProjectImage>().HasOne(x => x.MediaAsset).WithMany().HasForeignKey(x => x.MediaAssetId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<ProjectImage>().HasOne(x => x.SketchMediaAsset).WithMany().HasForeignKey(x => x.SketchMediaAssetId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<SiteSetting>().HasOne(x => x.MediaAsset).WithMany().HasForeignKey(x => x.MediaAssetId).OnDelete(DeleteBehavior.SetNull);
        b.Entity<SeoMeta>().HasOne(x => x.OgImage).WithMany().HasForeignKey(x => x.OgImageId).OnDelete(DeleteBehavior.SetNull);
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        foreach (var e in ChangeTracker.Entries<Entity>().Where(e => e.State == EntityState.Modified))
            e.Entity.UpdatedUtc = DateTime.UtcNow;
        return base.SaveChangesAsync(ct);
    }
}
