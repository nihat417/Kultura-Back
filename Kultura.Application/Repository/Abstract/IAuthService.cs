using Kultura.Application.Dto.AuthDto;
using static Kultura.Application.Model.Responses.ServiceResponses;

namespace Kultura.Application.Repository.Abstract
{
    public interface IAuthService
    {        
        Task<LoginResponse> Login(LoginDto loginDto);
        Task<GeneralResponse> Register(RegisterDto registerDto);
    }
}
