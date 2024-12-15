using Kultura.Application.Dto.AuthDto;
using Kultura.Application.Model;
using Kultura.Application.Repository.Abstract;
using Kultura.Domain.Entities;
using Kultura.Persistence.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static Kultura.Application.Model.Responses.ServiceResponses;


namespace Kultura.Application.Repository.Concrete
{
    public class AuthService(AppDbContext _dbContext,JwtTokenService _jwtTokenService) : IAuthService
    {
        #region Auth

        public async Task<LoginResponse> Login(LoginDto loginDto)
        {
            if (loginDto == null) return new LoginResponse(false, null, null, "Login model is empty");

            var getUser = await _dbContext.Users.FirstOrDefaultAsync(e => e.Email== loginDto.Email);

            if (getUser == null) return new LoginResponse(false, null!, null!, "User not found");

            var passwordHasher = new PasswordHasher<User>();
            var verificationResult = passwordHasher.VerifyHashedPassword(getUser, getUser.PasswordHash, loginDto.Password);
            if (verificationResult != PasswordVerificationResult.Success) return new LoginResponse(false, null, null, "Invalid email or password");

            var userSession = new UserSession(getUser.Id, getUser.Country,getUser.FullName, getUser.Age, getUser.Email,"User");
            (string accsesToken, string refreshToken) = _jwtTokenService.CreateToken(userSession);

            return new LoginResponse(true, accsesToken!, refreshToken, "Login completed");
        }

        public async Task<GeneralResponse> Register(RegisterDto registerDto)
        {
            if (registerDto == null) return new GeneralResponse(false, "Register DTO is Empty", null, null);

            var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == registerDto.Email);
            if (existingUser != null) return new GeneralResponse(false, null, "This email is already registered", null);

            var passwordHasher = new PasswordHasher<User>();

            var newUser = new User()
            {
                FullName = registerDto.FullName,
                Email = registerDto.Email,
                Age = registerDto.Age,
                PasswordHash = passwordHasher.HashPassword(null, registerDto.Password),
                Password = passwordHasher.HashPassword(null, registerDto.Password),
                CreatedTime = DateTime.Now,
                Roles = new() { Name = "User" }
            };

            ///*newUser.ImageUrl = (userDTO.ImageUrl != null) ? await UploadFileHelper.UploadFile(userDTO.ImageUrl!, "userphoto", newUser.Id) :
            //       "https://seventysoundst.blob.core.windows.net/userphoto/userdef.png";*/


            _dbContext.Users.Add(newUser);
            await _dbContext.SaveChangesAsync();

            return new GeneralResponse(true, "Account Created", null, null);
        }

        #endregion

        #region operation servicess

        public async Task<GeneralResponse> FindEmailUser(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return new GeneralResponse(false, null, "Email is required", null);

            var user = await _dbContext.Users.FirstOrDefaultAsync(r => r.Email == email);

            if (user == null) return new GeneralResponse(false, null, "User not found", null);

            return new GeneralResponse(true, "User found", null, user);
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _dbContext!.Users!.FirstOrDefaultAsync(r => r.Email == email);
        }

        #endregion

        #region email token services

        public async Task<GeneralResponse> GenerateEmailConfirmToken(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return new GeneralResponse(false, "Email is required", null, null);

            var user = await _dbContext.Users.FirstOrDefaultAsync(r => r.Email == email);
            if (user == null) return new GeneralResponse(false, "User not found", null, null);

            var token = await _jwtTokenService.GenerateEmailConfirmationTokenAsync(user);
            if (string.IsNullOrEmpty(token)) return new GeneralResponse(false, "Failed to generate email confirmation token", null, null);

            return new GeneralResponse(true, "Token generated", null, token);
        }

        public async Task<GeneralResponse> ConfirmEmail(string token, string email)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(email))
                return new GeneralResponse(false, "Token or email is required", null, null);

            var restaurant = await _dbContext.Restaurants.FirstOrDefaultAsync(r => r.Email == email);
            if (restaurant == null)
                return new GeneralResponse(false, "Restaurant not found", null, null);

            var isValidToken = _jwtTokenService.ValidateEmailConfirmationTokenAsync(token, restaurant);
            if (!isValidToken)
                return new GeneralResponse(false, "Invalid or expired token", null, null);

            restaurant.EmailConfirmed = true;
            await _dbContext.SaveChangesAsync();

            return new GeneralResponse(true, "Email confirmed successfully", null, null);
        }

        #endregion

        public async Task UpdateAsync(User user)
        {
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
        }


    }
}
