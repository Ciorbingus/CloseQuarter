namespace CloseQuarter.Shared.DTOs;

public class LoginRequestDTO
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
}

public class TokenLoginRequestDTO
{
    public string SessionToken { get; set; } = string.Empty;
}

public class AuthResponseDTO
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string SessionToken { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
}