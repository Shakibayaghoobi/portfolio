using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Dtos;
using Portfolio.Infrastructure;

namespace Portfolio.Api.Controllers.Admin;

/// <summary>Site settings (name, accent, contact details). Image settings are changed with the media picker in Phase 2.</summary>
[ApiController, Route("api/admin/site"), Authorize(Roles = "Admin")]
public partial class AdminSiteController(PortfolioDbContext db) : ControllerBase
{
    [GeneratedRegex("^#[0-9a-fA-F]{6}$")]
    private static partial Regex HexColor();

    [HttpGet]
    public async Task<List<AdminSettingDto>> List(CancellationToken ct)
    {
        var items = await db.SiteSettings.AsNoTracking().Include(s => s.MediaAsset).OrderBy(s => s.Key).ToListAsync(ct);
        return items.Select(s => new AdminSettingDto(s.Key, s.Value.ToDto(), s.MediaAsset?.ToDto())).ToList();
    }

    [HttpPut("{key}")]
    public async Task<ActionResult<AdminSettingDto>> Update(string key, LocalizedDto value, CancellationToken ct)
    {
        var setting = await db.SiteSettings.Include(s => s.MediaAsset).FirstOrDefaultAsync(s => s.Key == key, ct);
        if (setting is null) return NotFound();
        if (setting.MediaAssetId is not null)
            return BadRequest(new { error = "Image settings are changed with the media library (Phase 2)." });
        if (value.En.Length > 500 || value.Fa.Length > 500)
            return BadRequest(new { error = "Value is limited to 500 characters." });
        if (key == "site.accent" && !HexColor().IsMatch(value.En))
            return BadRequest(new { error = "Accent must be a hex colour like #f0c27a." });

        // Accent is a single value shared by both languages.
        setting.Value.En = value.En;
        setting.Value.Fa = key == "site.accent" ? value.En : value.Fa;
        await db.SaveChangesAsync(ct);
        return new AdminSettingDto(setting.Key, setting.Value.ToDto(), null);
    }
}
