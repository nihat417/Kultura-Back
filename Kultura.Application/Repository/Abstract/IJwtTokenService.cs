using Kultura.Application.Model;
using Kultura.Domain.Entities;

namespace Kultura.Application.Repository.Abstract
{
    public interface IJwtTokenService
    {
        bool ValidateEmailConfirmationTokenAsync(string token, Restaurant restaurant);
        Task<string> GenerateEmailConfirmationTokenAsync(Restaurant restaurant);
        (string accessToken, string refreshToken) CreateToken(UserSession user);
        string GetUserIdFromToken(string accessToken);
    }
}
