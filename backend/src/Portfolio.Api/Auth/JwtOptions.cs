namespace Portfolio.Api.Auth;

public class JwtOptions
{
    public const string Section = "Jwt";
    public string Issuer { get; set; } = "portfolio-api";
    public string Audience { get; set; } = "portfolio-admin";
    /// <summary>At least 32 chars. Override with env var Jwt__Key in every real environment.</summary>
    public string Key { get; set; } = "";
    public int ExpiresMinutes { get; set; } = 60;
}
