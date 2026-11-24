using Portfolio.Application.Dtos;
using Portfolio.Domain;

namespace Portfolio.Api;

internal static class Mapping
{
    public static LocalizedDto ToDto(this LocalizedText t) => new(t.En, t.Fa);

    /// <summary>Media URLs are stable ids; the endpoint that serves the files arrives in Phase 2.</summary>
    public static MediaRefDto ToDto(this MediaAsset m) => new(m.Id, $"/media/{m.Id}", m.Alt.ToDto());
}
