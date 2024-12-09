using Kultura.Application.Dto.RestaurntDtos;
using Kultura.Application.Model;
using Kultura.Application.Repository.Abstract;
using Kultura.Domain.Entities;
using Kultura.Persistence.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static Kultura.Application.Model.Responses.ServiceResponses;

namespace Kultura.Application.Repository.Concrete
{
    internal class RestaurantService(AppDbContext _dbContext, JwtTokenService _jwtTokenService) : IRestaurantService
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
                Roles = new() { Name = "Restaurant" },
                PasswordHash = passwordHasher.HashPassword(null, restaurantRegisterDto.Password),
                Password = passwordHasher.HashPassword(null, restaurantRegisterDto.Password)
            };

            _dbContext.Restaurants.Add(newRestaurant);
            await _dbContext.SaveChangesAsync();

            return new GeneralResponse(true, "Account Created", null, newRestaurant.Id);
        }


        public async Task<LoginResponse> LoginRestaurant(RestaurantLoginDto restaurantLogin)
        {
            if (restaurantLogin == null) return new LoginResponse(false, null, null, "Login model is empty");

            var restaurant = await _dbContext.Restaurants.FirstOrDefaultAsync(r => r.Email == restaurantLogin.Email);

            if (restaurant == null) return new LoginResponse(false, null, null, "Invalid email or password");

            var passwordHasher = new PasswordHasher<Restaurant>();
            var verificationResult = passwordHasher.VerifyHashedPassword(restaurant, restaurant.PasswordHash, restaurantLogin.Password);

            if (verificationResult != PasswordVerificationResult.Success) return new LoginResponse(false, null, null, "Invalid email or password");

            var restaurantsession = new UserSession(restaurant.Id, restaurant.Name,"",0,restaurant.Email,"Restaurant");
            (string accsesToken, string refreshToken) = _jwtTokenService.CreateToken(restaurantsession);

            return new LoginResponse(true,accsesToken, refreshToken, "Login successful");
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

        #region general services

        //get

        public async Task<GeneralResponse> GetRestaurantById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))return new GeneralResponse(false, null, "Restaurant ID is null or empty", null);

            try
            {
                var restaurant = await _dbContext.Restaurants.FirstOrDefaultAsync(r => r.Id == id);
                if (restaurant == null) return new GeneralResponse(false, null, "Restaurant not found", null);

                return new GeneralResponse(true, "Restaurant finded", null, restaurant);
            }
            catch (Exception ex)
            {
                return new GeneralResponse(false, null, $"Error retrieving restaurant: {ex.Message}", null);
            }
        }

        public async Task<GeneralResponse> GetRestaurantByEmail(string email)
        {
            if (!string.IsNullOrWhiteSpace(email)) return new GeneralResponse(false,null,"email is null", null);

            try
            {
                var restaurant = await _dbContext.Restaurants.FindAsync(email);
                if (restaurant == null) return new GeneralResponse(false, null, "Restaurant not found", null);
                return new GeneralResponse(true, "Restaurant finded", null, restaurant);
            }
            catch(Exception ex) 
            {
                return new GeneralResponse(false, null, $"Error retrieving restaurant: {ex.Message}", null);
            }
        }

        //post

        public async Task<GeneralResponse> AddFloor(FloorDto floordto)
        {
            if (floordto == null) return new GeneralResponse(false, null, "Floor Dto is null", null);

            var restaurant = await _dbContext.Restaurants.FirstOrDefaultAsync(r => r.Id == floordto.RestaurantId);
            if (restaurant == null) return new GeneralResponse(false, null, "Restaurant not found", null);

            var lowerOrEqualFloorExists = await _dbContext.Floors
                .AnyAsync(f => f.RestaurantId == floordto.RestaurantId && f.Number <= floordto.Number);

            if (!lowerOrEqualFloorExists && floordto.Number > 1)
                return new GeneralResponse(false, null, "Cannot add this floor. Lower-numbered floors must exist first.", null);

            try
            {
                var createdFloor = new Floor
                {
                    Number = floordto.Number,
                    RestaurantId = floordto.RestaurantId,
                    CreatedAt = DateTime.Now
                };

                await _dbContext.Floors.AddAsync(createdFloor);
                await _dbContext.SaveChangesAsync();

                return new GeneralResponse(true, "Floor added successfully", null, null);
            }
            catch (Exception ex)
            {
                return new GeneralResponse(false, null, ex.Message, null);
            }
        }

        public async Task<GeneralResponse> AddTable(TableDto tableDto)
        {
            if (tableDto == null) return new GeneralResponse(false, null, "Table Dto is null", null);

            var floor = await _dbContext.Floors.FirstOrDefaultAsync(f => f.Number== tableDto.FloorNumber);
            if(floor == null) return new GeneralResponse(false, null, "Restaurant not found", null);

            try
            {
                var createdTable = new Table
                {
                    RestaurantId = tableDto.RestaurantId,
                    FloorNumber = floor.Number,
                    Capacity = tableDto.Capacity,
                    ShapeType = tableDto.ShapeType,
                    X = tableDto.X,
                    Y = tableDto.Y,
                    Radius = tableDto.Radius,
                };
                await _dbContext.Tables.AddAsync(createdTable);
                await _dbContext.SaveChangesAsync();

                return new GeneralResponse(true, "Table added successfully", null, null);
            }
            catch (Exception ex)
            {
                return new GeneralResponse(false, null, ex.Message, null);
            }
        }


        //delete

        public async Task<GeneralResponse> DeleteFloor(FloorDto floordto)
        {
            if (floordto == null) return new GeneralResponse(false, null, "Floor Dto is null", null);

            try
            {
                var floor = await _dbContext.Floors.FirstOrDefaultAsync(f => f.RestaurantId == floordto.RestaurantId);

                if (floor == null)
                    return new GeneralResponse(false, null, "Floor not found or does not belong to the specified restaurant", null);

                var higherFloorExists = await _dbContext.Floors.AnyAsync(f => f.RestaurantId == floordto.RestaurantId && f.Number > floor.Number);

                if (higherFloorExists) return new GeneralResponse(false, null, "Cannot delete this floor. Please delete higher-numbered floors first.", null);

                 _dbContext.Floors.Remove(floor);
                await _dbContext.SaveChangesAsync();

                return new GeneralResponse(true, "Floor deleted successfully", null, null);
            }
            catch (Exception ex)
            {
                return new GeneralResponse(false, null, $"Error deleting floor: {ex.Message}", null);
            }
        }

        public async Task<GeneralResponse> DeleteTable(string tableId, string restaurantId)
        {
            try
            {
                var table = await _dbContext.Tables.FirstOrDefaultAsync(t => t.Id == tableId && t.RestaurantId == restaurantId);
                if (table == null)
                    return new GeneralResponse(false, null, "Table not found or does not belong to the specified restaurant", null);

                _dbContext.Tables.Remove(table);
                await _dbContext.SaveChangesAsync();

                return new GeneralResponse(true, "Table deleted successfully", null, null);
            }
            catch (Exception ex)
            {
                return new GeneralResponse(false, null, $"Error deleting table: {ex.Message}", null);
            }
        }



        #endregion


        public async Task UpdateAsync(Restaurant restaurant)
        {
            _dbContext.Restaurants.Update(restaurant);
            await _dbContext.SaveChangesAsync();
        }

        
    }
}
