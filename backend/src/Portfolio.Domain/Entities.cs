namespace Portfolio.Domain;

/// <summary>Bilingual text value (English + Persian). Stored as two columns via an owned type.</summary>
public class LocalizedText
{
    public string En { get; set; } = "";
    public string Fa { get; set; } = "";
}

public abstract class Entity
{
    public int Id { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;
}

/// <summary>Any piece of copy on the site (nav labels, headings, buttons...). Addressed by a stable key such as "hero.tag".</summary>
public class ContentEntry : Entity
{
    public string Key { get; set; } = "";
    public string? Group { get; set; }
    public LocalizedText Value { get; set; } = new();
}

/// <summary>Site-wide settings: name, accent colour, contact details, social links, logo and hero media.</summary>
public class SiteSetting : Entity
{
    public string Key { get; set; } = "";
    public LocalizedText Value { get; set; } = new();
    public int? MediaAssetId { get; set; }
    public MediaAsset? MediaAsset { get; set; }
}

/// <summary>A page-level section (about, works, studies...). Admin can reorder and hide sections.</summary>
public class Section : Entity
{
    public string Key { get; set; } = "";
    public int SortOrder { get; set; }
    public bool IsVisible { get; set; } = true;
}

/// <summary>Per-route SEO metadata.</summary>
public class SeoMeta : Entity
{
    public string Route { get; set; } = "/";
    public LocalizedText Title { get; set; } = new();
    public LocalizedText Description { get; set; } = new();
    public int? OgImageId { get; set; }
    public MediaAsset? OgImage { get; set; }
    public string? CanonicalUrl { get; set; }
    public bool NoIndex { get; set; }
}

public class MediaAsset : Entity
{
    public string FileName { get; set; } = "";
    public string ContentType { get; set; } = "";
    public long SizeBytes { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    /// <summary>Path relative to the storage root, e.g. "2026/11/abc123.webp".</summary>
    public string StoragePath { get; set; } = "";
    public LocalizedText Alt { get; set; } = new();
    public string? Folder { get; set; }
}

public class Project : Entity
{
    public string Slug { get; set; } = "";
    public string Number { get; set; } = "";
    public LocalizedText Title { get; set; } = new();
    public LocalizedText Type { get; set; } = new();
    public LocalizedText Note { get; set; } = new();
    public int SortOrder { get; set; }
    public bool IsPublished { get; set; } = true;
    public List<ProjectImage> Images { get; set; } = new();
}

public class ProjectImage : Entity
{
    public int ProjectId { get; set; }
    public Project? Project { get; set; }
    public int MediaAssetId { get; set; }
    public MediaAsset? MediaAsset { get; set; }
    public int? SketchMediaAssetId { get; set; }
    public MediaAsset? SketchMediaAsset { get; set; }
    public int SortOrder { get; set; }
}

public class Service : Entity
{
    public LocalizedText Title { get; set; } = new();
    public LocalizedText Text { get; set; } = new();
    public int SortOrder { get; set; }
}

public class Award : Entity
{
    public int Year { get; set; }
    public LocalizedText Title { get; set; } = new();
    public LocalizedText By { get; set; } = new();
    public int SortOrder { get; set; }
}

public class EducationItem : Entity
{
    public int FromYear { get; set; }
    public int? ToYear { get; set; }
    public LocalizedText Title { get; set; } = new();
    public LocalizedText Organization { get; set; } = new();
    public int SortOrder { get; set; }
}

public class ExperienceItem : Entity
{
    public int FromYear { get; set; }
    public int? ToYear { get; set; }
    public LocalizedText Organization { get; set; } = new();
    public LocalizedText Role { get; set; } = new();
    public LocalizedText Note { get; set; } = new();
    public int SortOrder { get; set; }
}

public class Skill : Entity
{
    public string Name { get; set; } = "";
    public int Value { get; set; }
    public int SortOrder { get; set; }
}

public class LanguageSkill : Entity
{
    public LocalizedText Name { get; set; } = new();
    public LocalizedText Level { get; set; } = new();
    public int SortOrder { get; set; }
}

public class ContactMessage : Entity
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string ProjectType { get; set; } = "";
    public string Message { get; set; } = "";
    public bool IsRead { get; set; }
}

public class AdminUser : Entity
{
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public DateTime? LastLoginUtc { get; set; }
}
