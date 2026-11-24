using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Dtos;
using Portfolio.Infrastructure;

namespace Portfolio.Api.Controllers;

/// <summary>Read-only, cacheable endpoints consumed by the public Angular site (SSR/prerender).</summary>
[ApiController, Route("api/public")]
public class PublicContentController(PortfolioDbContext db) : ControllerBase
{
    /// <summary>All copy keyed by content key: { "hero.tag": { en, fa }, ... }.</summary>
    [HttpGet("content"), ResponseCache(Duration = 60)]
    public async Task<Dictionary<string, LocalizedDto>> Content(CancellationToken ct)
    {
        var entries = await db.ContentEntries.AsNoTracking().ToListAsync(ct);
        return entries.ToDictionary(e => e.Key, e => e.Value.ToDto());
    }

    /// <summary>Site settings, media references and the ordered list of visible sections.</summary>
    [HttpGet("site"), ResponseCache(Duration = 60)]
    public async Task<SiteDto> Site(CancellationToken ct)
    {
        var settings = await db.SiteSettings.AsNoTracking().Include(s => s.MediaAsset).ToListAsync(ct);
        var sections = await db.Sections.AsNoTracking().Where(s => s.IsVisible).OrderBy(s => s.SortOrder).ToListAsync(ct);
        return new SiteDto(
            settings.Where(s => s.MediaAsset is null).ToDictionary(s => s.Key, s => s.Value.ToDto()),
            settings.Where(s => s.MediaAsset is not null).ToDictionary(s => s.Key, s => s.MediaAsset!.ToDto()),
            sections.Select(s => new SectionDto(s.Key, s.SortOrder)).ToList());
    }

    [HttpGet("projects"), ResponseCache(Duration = 60)]
    public async Task<List<ProjectDto>> Projects(CancellationToken ct)
    {
        var projects = await ProjectQuery().Where(p => p.IsPublished).OrderBy(p => p.SortOrder).ToListAsync(ct);
        return projects.Select(ToDto).ToList();
    }

    [HttpGet("projects/{slug}"), ResponseCache(Duration = 60)]
    public async Task<ActionResult<ProjectDto>> Project(string slug, CancellationToken ct)
    {
        var p = await ProjectQuery().FirstOrDefaultAsync(x => x.Slug == slug && x.IsPublished, ct);
        return p is null ? NotFound() : ToDto(p);
    }

    /// <summary>Services, awards, education, experience, skills and languages, each in admin-defined order.</summary>
    [HttpGet("cv"), ResponseCache(Duration = 60)]
    public async Task<CvDto> Cv(CancellationToken ct)
    {
        var services = await db.Services.AsNoTracking().OrderBy(x => x.SortOrder).ToListAsync(ct);
        var awards = await db.Awards.AsNoTracking().OrderBy(x => x.SortOrder).ToListAsync(ct);
        var education = await db.Education.AsNoTracking().OrderBy(x => x.SortOrder).ToListAsync(ct);
        var experience = await db.Experience.AsNoTracking().OrderBy(x => x.SortOrder).ToListAsync(ct);
        var skills = await db.Skills.AsNoTracking().OrderBy(x => x.SortOrder).ToListAsync(ct);
        var languages = await db.LanguageSkills.AsNoTracking().OrderBy(x => x.SortOrder).ToListAsync(ct);
        return new CvDto(
            services.Select(x => new ServiceDto(x.Title.ToDto(), x.Text.ToDto())).ToList(),
            awards.Select(x => new AwardDto(x.Year, x.Title.ToDto(), x.By.ToDto())).ToList(),
            education.Select(x => new EducationDto(x.FromYear, x.ToYear, x.Title.ToDto(), x.Organization.ToDto())).ToList(),
            experience.Select(x => new ExperienceDto(x.FromYear, x.ToYear, x.Organization.ToDto(), x.Role.ToDto(), x.Note.ToDto())).ToList(),
            skills.Select(x => new SkillDto(x.Name, x.Value)).ToList(),
            languages.Select(x => new LanguageDto(x.Name.ToDto(), x.Level.ToDto())).ToList());
    }

    private IQueryable<Portfolio.Domain.Project> ProjectQuery() =>
        db.Projects.AsNoTracking()
            .Include(p => p.Images).ThenInclude(i => i.MediaAsset)
            .Include(p => p.Images).ThenInclude(i => i.SketchMediaAsset);

    private static ProjectDto ToDto(Portfolio.Domain.Project p) => new(
        p.Slug, p.Number, p.Title.ToDto(), p.Type.ToDto(), p.Note.ToDto(),
        p.Images.OrderBy(i => i.SortOrder)
            .Select(i => new ProjectImageDto(i.MediaAsset!.ToDto(), i.SketchMediaAsset?.ToDto()))
            .ToList());
}
