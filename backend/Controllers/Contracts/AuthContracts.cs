using System.ComponentModel.DataAnnotations;

namespace dotnet_test.Controllers.Contracts;

public class LoginRequest
{
    [Required]
    [MaxLength(100)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? PlayerId { get; set; }
}
