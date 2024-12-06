using Kultura.Application.Dto.RestaurntDtos;
using Kultura.Application.Repository.Abstract;
using Kultura.Application.Services;
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

        public async Task<GeneralResponse> RegisterRestaurant(RestaurantRegisterDto restaurantRegisterDto)
        {
            if (restaurantRegisterDto == null) return new GeneralResponse(false, "Register DTO is Empty", null, null);

            var existingUser = await userManager.FindByEmailAsync(restaurantRegisterDto.Email);
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


            var restaurant = await userManager.FindByEmailAsync(restaurantRegisterDto.Email);
            if (restaurant != null) return new GeneralResponse(false, null, "This email already registered", null);

            var restaurantName = await userManager.FindByNameAsync(restaurantRegisterDto.Name);
            if (restaurantName != null) return new GeneralResponse(false, null, "This Username already registered", null);

            _dbContext.Restaurants.Add(newRestaurant);
            await _dbContext.SaveChangesAsync();

            return new GeneralResponse(true, "Account Created", null, newRestaurant.Id);
        }


        public async Task<GeneralResponse> LoginRestaurant(RestaurantLoginDto restaurantLogin)
        {

            throw new NotImplementedException();
        }

        public async Task<GeneralResponse> FindEmailRestaurant(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return new GeneralResponse(false, null, "Email is required", null);

            var restaurant = await _dbContext.Restaurants.FirstOrDefaultAsync(r => r.Email == email);

            if (restaurant == null) return new GeneralResponse(false, null, "Restaurant not found", null);

            return new GeneralResponse(true, "Restaurant found", null, restaurant);
        }


    }
}
