namespace Kultura.Application.Model.Responses
{
    public class ServiceResponses
    {
        public record class GeneralResponse(bool Success, string? Message, string? ErrorMessage, object? Data);
        public record class LoginResponse(bool Success, string AccsesToken, string RefreshToken, string? Message);
    }
}
