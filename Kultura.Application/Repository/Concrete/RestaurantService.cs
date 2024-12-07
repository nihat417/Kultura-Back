using Kultura.Application.Dto.RestaurntDtos;
using Kultura.Application.Repository.Abstract;
using Kultura.Domain.Entities;
using Kultura.Persistence.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static Kultura.Application.Model.Responses.ServiceResponses;

namespace Kultura.Application.Repository.Concrete
{
    internal class RestaurantService(UserManager<User> userManager, AppDbContext _dbContext, RoleManager<IdentityRole> roleManager, 
        JwtTokenService _jwtTokenService) : IRestaurantService
    {

        #region Auth Service

        public async Task<GeneralResponse> RegisterRestaurant(RestaurantRegisterDto restaurantRegisterDto)
        {
            if (restaurantRegisterDto == null) return new GeneralResponse(false, "Register DTO is Empty", null, null);

            var existingUser = await _dbContext.Restaurants.FirstOrDefaultAsync(r => r.Email == restaurantRegisterDto.Email);
            if (existingUser != null) return new GeneralResponse(false, null, "This email is already registered", null);

            var passwordHasher = new PasswordHasher<Restaurant>();
            var newRestaurant = new Restaurant
            {
                Name = restaurantRegisterDto.Name,
                Email = restaurantRegisterDto.Email,
                Location = restaurantRegisterDto.Location,
                Description = restaurantRegisterDto.Description,
                Cuisines = restaurantRegisterDto.Cuisines,
                PhoneNumber = restaurantRegisterDto.PhoneNumber,
                MinPrice = restaurantRegisterDto.MinPrice,
                MaxPrice = restaurantRegisterDto.MaxPrice,
                OpeningTime = restaurantRegisterDto.OpeningTime,
                ClosingTime = restaurantRegisterDto.ClosingTime,
                CreatedAt = DateTime.UtcNow,
                PasswordHash = passwordHasher.HashPassword(null, restaurantRegisterDto.Password),
                Password = passwordHasher.HashPassword(null, restaurantRegisterDto.Password)
            };

            _dbContext.Restaurants.Add(newRestaurant);
            await _dbContext.SaveChangesAsync();

            return new GeneralResponse(true, "Account Created", null, newRestaurant.Id);
        }


        public async Task<GeneralResponse> LoginRestaurant(RestaurantLoginDto restaurantLogin)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region operation services

        public async Task<GeneralResponse> FindEmailRestaurant(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return new GeneralResponse(false, null, "Email is required", null);

            var restaurant = await _dbContext.Restaurants.FirstOrDefaultAsync(r => r.Email == email);

            if (restaurant == null) return new GeneralResponse(false, null, "Restaurant not found", null);

            return new GeneralResponse(true, "Restaurant found", null, restaurant);
        }

        public async Task<Restaurant> GetByEmailAsync(string email)
        {
            return await _dbContext!.Restaurants!.FirstOrDefaultAsync(r => r.Email == email);
        }

        #endregion


        #region email token services

        public async Task<GeneralResponse> GenerateEmailConfirmToken(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return new GeneralResponse(false, "Email is required", null, null);

            var restaurant = await _dbContext.Restaurants.FirstOrDefaultAsync(r => r.Email == email);
            if (restaurant == null)
                return new GeneralResponse(false, "Restaurant not found", null, null);

            var token = await _jwtTokenService.GenerateEmailConfirmationTokenAsync(restaurant);
            if (string.IsNullOrEmpty(token))
                return new GeneralResponse(false, "Failed to generate email confirmation token", null, null);

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


        public async Task UpdateAsync(Restaurant restaurant)
        {
            _dbContext.Restaurants.Update(restaurant);
            await _dbContext.SaveChangesAsync();
        }
    }
}
