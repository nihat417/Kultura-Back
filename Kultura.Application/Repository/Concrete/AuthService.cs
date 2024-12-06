using Kultura.Application.Dto.AuthDto;
using Kultura.Application.Model;
using Kultura.Application.Model.Responses;
using Kultura.Application.Repository.Abstract;
using Kultura.Application.Services;
using Kultura.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using static Kultura.Application.Model.Responses.ServiceResponses;


namespace Kultura.Application.Repository.Concrete
{
    public class AuthService(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, JwtTokenService _jwtTokenService) : IAuthService
    {
        #region login

        public async Task<LoginResponse> Login(LoginDto loginDto)
        {
            if (loginDto == null) return new LoginResponse(false, null, null, "Login model is empty");

            var getUser = await userManager.FindByEmailAsync(loginDto.Email);

            if (getUser == null) return new LoginResponse(false, null!, null!, "User not found");

            bool checkUserPasswords = await userManager.CheckPasswordAsync(getUser, loginDto.Password);
            if (!checkUserPasswords) return new LoginResponse(false, null!, null!, "Invalid email/password");

            var getUserRole = await userManager.GetRolesAsync(getUser);
            var userSession = new UserSession(getUser.Id, getUser.UserName, getUser.FullName, getUser.Age, getUser.Email, getUserRole.First());
            (string accsesToken, string refreshToken) = _jwtTokenService.CreateToken(userSession);

            return new LoginResponse(true, accsesToken!, refreshToken, "Login completed");
        }

        #endregion

        #region register

        public async Task<GeneralResponse> Register(RegisterDto registerDto)
        {
            if (registerDto == null) return new GeneralResponse(false, "Register DTO is Empty",null,null);

            var newUser = new User()
            {
                UserName = registerDto.UserName,
                FullName = registerDto.FullName,
                Email = registerDto.Email,
                Age = registerDto.Age,
                PasswordHash = registerDto.Password,
                CreatedTime = DateTime.Now,
            };

            /*newUser.ImageUrl = (userDTO.ImageUrl != null) ? await UploadFileHelper.UploadFile(userDTO.ImageUrl!, "userphoto", newUser.Id) :
                   "https://seventysoundst.blob.core.windows.net/userphoto/userdef.png";*/

            var user = await userManager.FindByEmailAsync(registerDto.Email);
            if (user != null) return new GeneralResponse(false,null, "This email already registered", null);

            var userUsername = await userManager.FindByNameAsync(registerDto.UserName);
            if (userUsername != null) return new GeneralResponse(false,null, "This Username already registered", null);

            var createUser = await userManager.CreateAsync(newUser!, registerDto.Password);
            if (!createUser.Succeeded) return new GeneralResponse(false,null,"Error occured.. please try again",null);

            var checkUser = await roleManager.FindByNameAsync("User");
            if (checkUser is null) await roleManager.CreateAsync(new IdentityRole() { Name = "User" });

            await userManager.AddToRoleAsync(newUser, "User");
            return new GeneralResponse(true, "Account Created",null,null);
        }

        #endregion
    }
}
