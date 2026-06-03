namespace dotnet_test.Services.Auth;

public class AuthOptions
{
    public const string SectionName = "Auth";

    public string Issuer { get; set; } = "dotnet-test-api";
    public string Audience { get; set; } = "dotnet-test-frontend";
    public string SigningKey { get; set; } = "dev-change-this-signing-key-to-a-32-byte-minimum";
    public int AccessTokenMinutes { get; set; } = 120;
}
