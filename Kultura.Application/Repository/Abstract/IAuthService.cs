using Kultura.Application.Dto.AuthDto;
using Kultura.Domain.Entities;
using static Kultura.Application.Model.Responses.ServiceResponses;

namespace Kultura.Application.Repository.Abstract
{
    public interface IAuthService
    {        
        Task<LoginResponse> Login(LoginDto loginDto);
        Task<GeneralResponse> Register(RegisterDto registerDto);

        //operation services
        Task<GeneralResponse> FindEmailUser(string email);
        Task<User> GetByEmailAsync(string email);

        //token services
        Task<GeneralResponse> GenerateEmailConfirmToken(string email);
        Task<GeneralResponse> ConfirmEmail(string token, string email);

        Task UpdateAsync(User user);
    }
}
