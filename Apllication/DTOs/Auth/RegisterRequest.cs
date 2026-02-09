namespace Application.DTOs.Auth
{

    public sealed record RegisterRequest(string Email, string Name, string Password);

}
