namespace Portfolio.Application.Dtos;

public record LocalizedDto(string En, string Fa);

/// <summary>Reference to a media asset. Url points at the media endpoint added in Phase 2.</summary>
public record MediaRefDto(int Id, string Url, LocalizedDto Alt);

public record SectionDto(string Key, int SortOrder);

public record SiteDto(
    Dictionary<string, LocalizedDto> Settings,
    Dictionary<string, MediaRefDto> Media,
    List<SectionDto> Sections);

public record ProjectImageDto(MediaRefDto Render, MediaRefDto? Sketch);

public record ProjectDto(
    string Slug, string Number, LocalizedDto Title, LocalizedDto Type, LocalizedDto Note,
    List<ProjectImageDto> Images);

public record ServiceDto(LocalizedDto Title, LocalizedDto Text);
public record AwardDto(int Year, LocalizedDto Title, LocalizedDto By);
public record EducationDto(int From, int? To, LocalizedDto Title, LocalizedDto Organization);
public record ExperienceDto(int From, int? To, LocalizedDto Organization, LocalizedDto Role, LocalizedDto Note);
public record SkillDto(string Name, int Value);
public record LanguageDto(LocalizedDto Name, LocalizedDto Level);

public record CvDto(
    List<ServiceDto> Services, List<AwardDto> Awards, List<EducationDto> Education,
    List<ExperienceDto> Experience, List<SkillDto> Skills, List<LanguageDto> Languages);

// ---- admin ----
public record AdminContentEntryDto(int Id, string Key, string? Group, LocalizedDto Value);
public record AdminSettingDto(string Key, LocalizedDto Value, MediaRefDto? Media);
public record AdminSectionDto(string Key, int SortOrder, bool IsVisible);
public record SectionOrderItem(string Key, bool IsVisible);
