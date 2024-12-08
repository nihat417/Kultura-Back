using Kultura.Application.Model;
using Kultura.Domain.Entities;

namespace Kultura.Application.Repository.Abstract
{
    public interface IJwtTokenService
    {
        bool ValidateEmailConfirmationTokenAsync<T>(string token, T entity) where T : class;
        Task<string> GenerateEmailConfirmationTokenAsync<T>(T entity) where T : class;
        (string accessToken, string refreshToken) CreateToken(UserSession user);
        string GetUserIdFromToken(string accessToken);
    }
}
