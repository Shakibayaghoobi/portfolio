using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Portfolio.Api.Controllers;
using Portfolio.Api.Controllers.Admin;
using Portfolio.Application.Dtos;
using Portfolio.Infrastructure;
using Portfolio.Infrastructure.Seed;

namespace Portfolio.Tests;

public class ContentSeederTests
{
    /// <summary>SQLite in memory: a real relational provider, so the bilingual columns and FKs behave like SQL Server.</summary>
    private sealed class TestDb : IAsyncDisposable
    {
        private readonly SqliteConnection _connection = new("DataSource=:memory:");
        public PortfolioDbContext Db { get; }

        public TestDb()
        {
            _connection.Open();
            Db = new PortfolioDbContext(new DbContextOptionsBuilder<PortfolioDbContext>().UseSqlite(_connection).Options);
            Db.Database.EnsureCreated();
        }

        public async ValueTask DisposeAsync() { await Db.DisposeAsync(); await _connection.DisposeAsync(); }
    }

    [Fact]
    public async Task Seeds_everything_from_reference_content()
    {
        await using var testDb = new TestDb();
        var db = testDb.Db;
        await ContentSeeder.SeedAsync(db);

        Assert.True(await db.ContentEntries.CountAsync() >= 86);
        Assert.Equal(10, await db.Projects.CountAsync());
        Assert.Equal(9, await db.Sections.CountAsync());
        Assert.Equal(6, await db.Services.CountAsync());
        Assert.Equal(5, await db.Awards.CountAsync());
        Assert.Equal(10, await db.Experience.CountAsync());
        Assert.Equal(8, await db.Skills.CountAsync());
        Assert.Equal(4, await db.LanguageSkills.CountAsync());
        Assert.All(await db.MediaAssets.ToListAsync(), m => Assert.Equal(ContentSeeder.PendingFolder, m.Folder));

        var tag = await db.ContentEntries.SingleAsync(e => e.Key == "hero.tag");
        Assert.NotEmpty(tag.Value.En);
        Assert.NotEmpty(tag.Value.Fa);
        var arch = await db.Projects.Include(p => p.Images).SingleAsync(p => p.Slug == "arch-tower");
        Assert.Equal(4, arch.Images.Count);
        Assert.All(arch.Images, i => Assert.NotNull(i.SketchMediaAssetId));
    }

    [Fact]
    public async Task Seeding_twice_does_not_duplicate_or_overwrite_edits()
    {
        await using var testDb = new TestDb();
        var db = testDb.Db;
        await ContentSeeder.SeedAsync(db);
        var counts = (await db.ContentEntries.CountAsync(), await db.Projects.CountAsync(), await db.MediaAssets.CountAsync(), await db.SiteSettings.CountAsync());

        var entry = await db.ContentEntries.SingleAsync(e => e.Key == "hero.tag");
        entry.Value.En = "Edited in admin";
        await db.SaveChangesAsync();

        await ContentSeeder.SeedAsync(db);

        Assert.Equal(counts, (await db.ContentEntries.CountAsync(), await db.Projects.CountAsync(), await db.MediaAssets.CountAsync(), await db.SiteSettings.CountAsync()));
        Assert.Equal("Edited in admin", (await db.ContentEntries.SingleAsync(e => e.Key == "hero.tag")).Value.En);
    }

    [Fact]
    public async Task Public_endpoints_return_seeded_data()
    {
        await using var testDb = new TestDb();
        var db = testDb.Db;
        await ContentSeeder.SeedAsync(db);
        var api = new PublicContentController(db);

        var content = await api.Content(default);
        Assert.Contains("about.p1", content.Keys);

        var site = await api.Site(default);
        Assert.Equal("#f0c27a", site.Settings["site.accent"].En);
        Assert.Contains("media.hero", site.Media.Keys);
        Assert.Equal("hero", site.Sections[0].Key);

        var projects = await api.Projects(default);
        Assert.Equal(10, projects.Count);
        Assert.Equal("A-01", projects[0].Number);

        var one = await api.Project("villa", default);
        Assert.NotNull(one.Value);
        Assert.IsType<Microsoft.AspNetCore.Mvc.NotFoundResult>((await api.Project("nope", default)).Result);

        var cv = await api.Cv(default);
        Assert.Equal(8, cv.Skills.Count);
        Assert.Null(cv.Experience.First(e => e.Organization.En == "Elfi Group").To);
    }

    [Fact]
    public async Task Admin_can_edit_text_site_setting_and_sections()
    {
        await using var testDb = new TestDb();
        var db = testDb.Db;
        await ContentSeeder.SeedAsync(db);

        var content = new AdminContentController(db);
        var updated = await content.Update("hero.cta1", new LocalizedDto("Hi", "سلام"), default);
        Assert.Equal("Hi", updated.Value!.Value.En);
        Assert.IsType<Microsoft.AspNetCore.Mvc.NotFoundResult>((await content.Update("missing", new LocalizedDto("a", "b"), default)).Result);

        var site = new AdminSiteController(db);
        Assert.IsType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>((await site.Update("site.accent", new LocalizedDto("red", "red"), default)).Result);
        Assert.IsType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>((await site.Update("media.hero", new LocalizedDto("x", "x"), default)).Result);
        var ok = await site.Update("site.accent", new LocalizedDto("#112233", ""), default);
        Assert.Equal("#112233", ok.Value!.Value.Fa);

        var sections = new AdminSectionsController(db);
        var current = await sections.List(default);
        var reordered = current.AsEnumerable().Reverse().Select(s => new SectionOrderItem(s.Key, s.Key != "contact")).ToList();
        var saved = await sections.Save(reordered, default);
        Assert.Equal("contact", saved.Value![0].Key);
        Assert.False(saved.Value[0].IsVisible);
        Assert.True(saved.Value[1].IsVisible);
        var publicSections = (await new PublicContentController(db).Site(default)).Sections;
        Assert.DoesNotContain(publicSections, x => x.Key == "contact");
        Assert.Equal(8, publicSections.Count);
        Assert.IsType<Microsoft.AspNetCore.Mvc.BadRequestObjectResult>((await sections.Save(reordered.Take(2).ToList(), default)).Result);
    }
}
