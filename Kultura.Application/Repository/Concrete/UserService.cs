using Kultura.Application.Dto;
using Kultura.Application.Repository.Abstract;
using Kultura.Domain.Entities;
using Kultura.Persistence.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using static Kultura.Application.Model.Responses.ServiceResponses;

namespace Kultura.Application.Repository.Concrete
{
    public class UserService(AppDbContext _dbContext) : IUserService
    {

        #region get

        public async Task<GeneralResponse> GetAllFavourites(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return new GeneralResponse(false, null, "User ID cannot be empty.", null);
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return new GeneralResponse(false, null, "user not found.", null);

            var favourites = await _dbContext.Favourites
                .Include(f => f.Restaurant)
                .Where(f => f.UserId == userId)
                .Select(f => new
                {
                    f.RestaurantId,
                    f.Restaurant.Name,
                    f.Restaurant.Email,
                    f.Restaurant.Description,
                    f.Restaurant.PhoneNumber,
                    f.Restaurant.Cuisines,
                    f.Restaurant.ClosingTime,
                    RestaurantImage = f.Restaurant.MainPhoto,
                })
                .ToListAsync();

            if (favourites == null || !favourites.Any())
                return new GeneralResponse(false, null, "No favourites found.", null);

            return new GeneralResponse(true, "Favourites retrieved successfully.", null, favourites);
        }



        #endregion

        #region post

        public async Task<GeneralResponse> AddReservationAsync(ReservationDto reservationDto)
        {
            var table = await _dbContext.Tables.FindAsync(reservationDto.TableId);
            if (table == null)
                return new GeneralResponse(false, null, "Table not found.", null);

            var slot = await _dbContext.ReservationSlots.FindAsync(reservationDto.SlotId);
            if (slot == null || slot.IsReserved)
                return new GeneralResponse(false, null, "Slot is not available.", null);

            var existingReservation = await _dbContext.Reservations.FirstOrDefaultAsync(r => r.SlotId == reservationDto.SlotId);

            if (existingReservation != null)
                return new GeneralResponse(false, null, "Slot is already reserved by another user.", null);

            var currentDate = DateTime.UtcNow.Date;
            var currentTime = DateTime.UtcNow.TimeOfDay;

            if (reservationDto.ReservationDate < currentDate)
                return new GeneralResponse(false, null, "Reservation date cannot be in the past.", null);

            if (reservationDto.ReservationDate == currentDate && slot.StartTime < currentTime)
                return new GeneralResponse(false, null, "Reservation time cannot be in the past.", null);

            var reservation = new Reservation
            {
                TableId = reservationDto.TableId,
                UserId = reservationDto.UserId,
                ReservationDate = reservationDto.ReservationDate,
                SlotId = reservationDto.SlotId,
            };

            slot.IsReserved = true;

            await _dbContext.Reservations.AddAsync(reservation);
            await _dbContext.SaveChangesAsync();

            return new GeneralResponse(true, "Reservation added successfully.", null, reservation);
        }

        public async Task<GeneralResponse> SearchRestaurantsAsync(string search)
        {
            if (string.IsNullOrEmpty(search))
                return new GeneralResponse(false, null, "Search query cannot be empty.", null);

            var query = @"
                        SELECT 
                            r.Location, 
                            r.Description, 
                            r.MainPhoto, 
                            r.Name, 
                            r.PhoneNumber, 
                            r.Cuisines, 
                            r.Photos 
                        FROM Restaurants r
                        WHERE r.Name LIKE @query OR r.Description LIKE @query";

            var restaurants = await _dbContext.Restaurants
                .FromSqlRaw(query, new SqlParameter("@query", $"%{search}%"))
                .Select(r => new
                {
                    r.Location,
                    r.Description,
                    r.MainPhoto,
                    r.Name,
                    r.PhoneNumber,
                    r.Cuisines,
                    r.Photos 
                })
                .ToListAsync();

            if (restaurants == null || !restaurants.Any())
                return new GeneralResponse(false, null, "No restaurants found matching the search criteria.", null);

            return new GeneralResponse(true, "Restaurants found successfully.", null, restaurants);
        }

        public async Task<GeneralResponse> AddFavourites(string userId,string restaurantId)
        {
            if(string.IsNullOrEmpty(restaurantId)) return new GeneralResponse(false, null, "restaurant id cannot be empty.", null);

            var restaurant = await _dbContext.Restaurants.FirstOrDefaultAsync(r => r.Id == restaurantId);
            if (restaurant == null) return new GeneralResponse(false, null, "restaurant not found.", null);

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (string.IsNullOrEmpty(userId) || user == null) return new GeneralResponse(false, null, "User is not authenticated.", null);

            var existingFavourite = await _dbContext.Favourites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.RestaurantId == restaurantId);

            if (existingFavourite != null)
                return new GeneralResponse(false, null, "This restaurant is already in your favourites.", null);

            var favourite = new Favourite
            {
                UserId = userId,
                RestaurantId = restaurantId
            };

            _dbContext.Favourites.Add(favourite);
            await _dbContext.SaveChangesAsync();

            return new GeneralResponse(true, "Restaurant added to favourites successfully.",null, favourite);

        }


        #endregion

        #region delete

        public async Task<GeneralResponse> DeleteReservationAsync(string reservationId)
        {
            var reservation = await _dbContext.Reservations
                .Include(r => r.Slot)
                .FirstOrDefaultAsync(r => r.Id == reservationId);

            if (reservation == null)
            {
                return new GeneralResponse(false, null, "Reservation not found.", null);
            }

            if (reservation.Slot != null)
            {
                reservation.Slot.IsReserved = false;
            }

            _dbContext.Reservations.Remove(reservation);
            await _dbContext.SaveChangesAsync();

            return new GeneralResponse(true, "Reservation deleted successfully.", null, null);
        }

        public async Task<GeneralResponse> DeleteFavourite(string userId, string restaurantId)
        {
            if (string.IsNullOrEmpty(restaurantId))
                return new GeneralResponse(false, null, "Restaurant ID cannot be empty.", null);

            var favourite = await _dbContext.Favourites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.RestaurantId == restaurantId);

            if (favourite == null)
                return new GeneralResponse(false, null, "Favourite not found.", null);

            _dbContext.Favourites.Remove(favourite);
            await _dbContext.SaveChangesAsync();

            return new GeneralResponse(true, "Favourite removed successfully.", null, null);
        }


        #endregion

    }
}
