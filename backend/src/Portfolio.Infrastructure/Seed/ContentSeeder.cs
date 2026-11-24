using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Portfolio.Domain;

namespace Portfolio.Infrastructure.Seed;

/// <summary>
/// Loads the reference content (embedded Seed/content.json) into the database.
/// Idempotent and non-destructive: existing keys and non-empty tables are never overwritten, so admin edits survive restarts.
/// Images in the reference are hashes; they are seeded as placeholder MediaAssets (Folder = "seed-pending", no file yet).
/// Real files are attached in Phase 2.
/// </summary>
public static class ContentSeeder
{
    public const string PendingFolder = "seed-pending";
    private static readonly string[] MediaKeys = ["portrait", "hero", "dayImg", "nightImg", "film", "poster", "logo"];
    private static readonly string[] SectionKeys = ["hero", "about", "works", "studies", "practice", "film", "recognition", "path", "contact"];

    public static async Task SeedAsync(PortfolioDbContext db, ILogger? logger = null, CancellationToken ct = default)
    {
        using var doc = JsonDocument.Parse(ReadEmbedded());
        var root = doc.RootElement;

        var media = await SeedMediaAsync(db, root, ct);
        await SeedSettingsAsync(db, root, media, ct);
        await SeedContentAsync(db, root, ct);
        await SeedSectionsAsync(db, ct);
        await SeedProjectsAsync(db, root, media, ct);
        await SeedCvAsync(db, root, ct);
        logger?.LogInformation("Content seed finished.");
    }

    private static string ReadEmbedded()
    {
        var asm = typeof(ContentSeeder).Assembly;
        var name = asm.GetManifestResourceNames().Single(n => n.EndsWith("content.json", StringComparison.OrdinalIgnoreCase));
        using var stream = asm.GetManifestResourceStream(name)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    // ---- helpers -------------------------------------------------------------------------------------------------

    private static LocalizedText L(JsonElement e) => new()
    {
        En = e.GetProperty("en").GetString() ?? "",
        Fa = e.GetProperty("fa").GetString() ?? "",
    };

    private static LocalizedText Same(string v) => new() { En = v, Fa = v };

    private static int? OptInt(JsonElement e, string name) =>
        e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetInt32() : null;

    private static string? OptString(JsonElement e, string name) =>
        e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

    private static string GroupOf(string key)
    {
        var i = key.IndexOf('.');
        return i > 0 ? key[..i] : "site";
    }

    // ---- media placeholders --------------------------------------------------------------------------------------

    private static async Task<Dictionary<string, MediaAsset>> SeedMediaAsync(PortfolioDbContext db, JsonElement root, CancellationToken ct)
    {
        var hashes = new HashSet<string>();
        var site = root.GetProperty("site");
        foreach (var k in MediaKeys)
            if (OptString(site, k) is { Length: > 0 } h) hashes.Add(h);
        foreach (var p in root.GetProperty("projects").EnumerateArray())
            foreach (var img in p.GetProperty("images").EnumerateArray())
            {
                if (OptString(img, "src") is { Length: > 0 } s) hashes.Add(s);
                if (OptString(img, "sketch") is { Length: > 0 } k) hashes.Add(k);
            }

        var list = hashes.ToList();
        var existing = await db.MediaAssets.Where(m => list.Contains(m.FileName)).ToDictionaryAsync(m => m.FileName, ct);
        foreach (var hash in list.Where(h => !existing.ContainsKey(h)))
        {
            var asset = new MediaAsset
            {
                FileName = hash,
                ContentType = "application/octet-stream",
                StoragePath = $"seed/{hash}",
                Folder = PendingFolder,
            };
            db.MediaAssets.Add(asset);
            existing[hash] = asset;
        }
        await db.SaveChangesAsync(ct);
        return existing;
    }

    // ---- site settings -------------------------------------------------------------------------------------------

    private static async Task SeedSettingsAsync(PortfolioDbContext db, JsonElement root, Dictionary<string, MediaAsset> media, CancellationToken ct)
    {
        var keys = (await db.SiteSettings.Select(s => s.Key).ToListAsync(ct)).ToHashSet();
        void Add(string key, LocalizedText value, int? mediaId = null)
        {
            if (keys.Add(key)) db.SiteSettings.Add(new SiteSetting { Key = key, Value = value, MediaAssetId = mediaId });
        }

        var site = root.GetProperty("site");
        Add("site.name", L(site.GetProperty("name")));
        Add("site.first", L(site.GetProperty("first")));
        Add("site.last", L(site.GetProperty("last")));
        Add("site.accent", Same(site.GetProperty("accent").GetString() ?? "#f0c27a"));
        foreach (var k in MediaKeys)
            if (OptString(site, k) is { Length: > 0 } hash)
                Add($"media.{k}", new LocalizedText(), media[hash].Id);

        var c = root.GetProperty("contact");
        Add("contact.email", Same(c.GetProperty("email").GetString() ?? ""));
        Add("contact.phone", Same(c.GetProperty("phone").GetString() ?? ""));
        Add("contact.phoneText", Same(c.GetProperty("phoneText").GetString() ?? ""));
        Add("contact.instagram", Same(c.GetProperty("instagram").GetString() ?? ""));
        Add("contact.address", L(c.GetProperty("address")));

        await db.SaveChangesAsync(ct);
    }

    // ---- copy ----------------------------------------------------------------------------------------------------

    private static async Task SeedContentAsync(PortfolioDbContext db, JsonElement root, CancellationToken ct)
    {
        var keys = (await db.ContentEntries.Select(e => e.Key).ToListAsync(ct)).ToHashSet();
        void Add(string key, string group, LocalizedText value)
        {
            if (keys.Add(key)) db.ContentEntries.Add(new ContentEntry { Key = key, Group = group, Value = value });
        }

        foreach (var p in root.GetProperty("ui").EnumerateObject())
            Add(p.Name, GroupOf(p.Name), L(p.Value));

        var research = root.GetProperty("research");
        Add("research.title", "research", L(research.GetProperty("title")));
        Add("research.cite", "research", L(research.GetProperty("cite")));
        Add("research.year", "research", Same(research.GetProperty("year").GetInt32().ToString()));

        var i = 1;
        foreach (var t in root.GetProperty("contact").GetProperty("projectTypes").EnumerateArray())
            Add($"contact.projectType.{i++}", "contact", L(t));

        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedSectionsAsync(PortfolioDbContext db, CancellationToken ct)
    {
        if (await db.Sections.AnyAsync(ct)) return;
        for (var i = 0; i < SectionKeys.Length; i++)
            db.Sections.Add(new Section { Key = SectionKeys[i], SortOrder = i, IsVisible = true });
        await db.SaveChangesAsync(ct);
    }

    // ---- projects ------------------------------------------------------------------------------------------------

    private static async Task SeedProjectsAsync(PortfolioDbContext db, JsonElement root, Dictionary<string, MediaAsset> media, CancellationToken ct)
    {
        if (await db.Projects.AnyAsync(ct)) return;
        var order = 0;
        foreach (var p in root.GetProperty("projects").EnumerateArray())
        {
            var project = new Project
            {
                Slug = p.GetProperty("id").GetString()!,
                Number = p.GetProperty("no").GetString() ?? "",
                Title = L(p.GetProperty("title")),
                Type = L(p.GetProperty("type")),
                Note = L(p.GetProperty("note")),
                SortOrder = order++,
            };
            var imgOrder = 0;
            foreach (var img in p.GetProperty("images").EnumerateArray())
            {
                var sketch = OptString(img, "sketch");
                project.Images.Add(new ProjectImage
                {
                    MediaAssetId = media[img.GetProperty("src").GetString()!].Id,
                    SketchMediaAssetId = sketch is { Length: > 0 } ? media[sketch].Id : null,
                    SortOrder = imgOrder++,
                });
            }
            db.Projects.Add(project);
        }
        await db.SaveChangesAsync(ct);
    }

    // ---- CV lists ------------------------------------------------------------------------------------------------

    private static async Task SeedCvAsync(PortfolioDbContext db, JsonElement root, CancellationToken ct)
    {
        if (!await db.Services.AnyAsync(ct))
        {
            var i = 0;
            foreach (var s in root.GetProperty("services").EnumerateArray())
                db.Services.Add(new Service { Title = L(s.GetProperty("title")), Text = L(s.GetProperty("text")), SortOrder = i++ });
        }
        if (!await db.Awards.AnyAsync(ct))
        {
            var i = 0;
            foreach (var a in root.GetProperty("awards").EnumerateArray())
                db.Awards.Add(new Award { Year = a.GetProperty("year").GetInt32(), Title = L(a.GetProperty("title")), By = L(a.GetProperty("by")), SortOrder = i++ });
        }
        if (!await db.Education.AnyAsync(ct))
        {
            var i = 0;
            foreach (var e in root.GetProperty("education").EnumerateArray())
                db.Education.Add(new EducationItem
                {
                    FromYear = e.GetProperty("from").GetInt32(), ToYear = OptInt(e, "to"),
                    Title = L(e.GetProperty("title")), Organization = L(e.GetProperty("org")), SortOrder = i++,
                });
        }
        if (!await db.Experience.AnyAsync(ct))
        {
            var i = 0;
            foreach (var e in root.GetProperty("experience").EnumerateArray())
                db.Experience.Add(new ExperienceItem
                {
                    FromYear = e.GetProperty("from").GetInt32(), ToYear = OptInt(e, "to"),
                    Organization = L(e.GetProperty("org")), Role = L(e.GetProperty("role")), Note = L(e.GetProperty("note")), SortOrder = i++,
                });
        }
        if (!await db.Skills.AnyAsync(ct))
        {
            var i = 0;
            foreach (var s in root.GetProperty("skills").EnumerateArray())
                db.Skills.Add(new Skill { Name = s.GetProperty("name").GetString() ?? "", Value = s.GetProperty("value").GetInt32(), SortOrder = i++ });
        }
        if (!await db.LanguageSkills.AnyAsync(ct))
        {
            var i = 0;
            foreach (var l in root.GetProperty("languages").EnumerateArray())
                db.LanguageSkills.Add(new LanguageSkill { Name = L(l.GetProperty("name")), Level = L(l.GetProperty("level")), SortOrder = i++ });
        }
        await db.SaveChangesAsync(ct);
    }
}
