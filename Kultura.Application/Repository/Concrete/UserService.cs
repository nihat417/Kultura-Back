using Kultura.Application.Dto;
using Kultura.Application.Repository.Abstract;
using Kultura.Domain.Entities;
using Kultura.Domain.Enums;
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

        public async Task<GeneralResponse> GetReserveHistoryAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return new GeneralResponse(false, null, "User ID cannot be empty.", null);

            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
                return new GeneralResponse(false, null, "User not found.", null);

            var reservations = await _dbContext.Reservations
                .Where(r => r.UserId == userId)
                .Include(r => r.Table)
                .ThenInclude(t => t.Floor)
                .Include(r => r.Slot)
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync();

            if (!reservations.Any())
                return new GeneralResponse(false, null, "No reservations found.", null);

            var reservationHistory = reservations.Select(r => new
            {
                r.Id,
                r.ReservationDate,
                r.Status,
                //RestaurantName = r.Table?.Restaurant?.Name,
                //RestaurantAddress = r.Table?.Restaurant?.Location,
                //RestaurantMoneyMin = r.Table?.Restaurant?.MinPrice,
                //RestaurantMoneyMax = r.Table?.Restaurant?.MaxPrice,
                SlotStartTime = r.Slot?.StartTime,
                SlotEndTime = r.Slot?.EndTime
            });

            return new GeneralResponse(true, "Reservation history retrieved successfully.", null, reservationHistory);
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

            var existingReservation = await _dbContext.Reservations
                .FirstOrDefaultAsync(r =>
                    r.UserId == reservationDto.UserId &&
                    r.Status == ReservationStatus.Active &&
                    r.ReservationDate == reservationDto.ReservationDate &&
                    r.TableId == reservationDto.TableId &&
                    r.SlotId == reservationDto.SlotId);

            if (existingReservation != null)
                return new GeneralResponse(false, null, "Slot is already reserved by the user.", null);

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
                Status = ReservationStatus.Active
            };

            slot.IsReserved = true;

            await _dbContext.Reservations.AddAsync(reservation);
            await _dbContext.SaveChangesAsync();

            return new GeneralResponse(true, "Reservation added successfully.", null, reservation);
        }

        public async Task<GeneralResponse> CancelReservationAsync(string reservationId)
        {
            var reservation = await _dbContext.Reservations.FindAsync(reservationId);
            if (reservation == null)
                return new GeneralResponse(false, null, "Reservation not found.", null);

            if (reservation.Status == ReservationStatus.Canceled)
                return new GeneralResponse(false, null, "Reservation is already canceled.", null);

            reservation.Status = ReservationStatus.Canceled;

            var slot = await _dbContext.ReservationSlots.FindAsync(reservation.SlotId);
            if (slot != null)
                slot.IsReserved = false;

            await _dbContext.SaveChangesAsync();

            return new GeneralResponse(true, "Reservation canceled successfully.", null, reservation);
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

        public async Task<GeneralResponse> AddReviewAsync(string userId, string restaurantId, string comment, int rating)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return new GeneralResponse(false, null, "User ID cannot be null or empty.", null);

            if (string.IsNullOrWhiteSpace(restaurantId))
                return new GeneralResponse(false, null, "Restaurant ID cannot be null or empty.", null);

            if (string.IsNullOrWhiteSpace(comment))
                return new GeneralResponse(false, null, "Comment cannot be null or empty.", null);

            if (rating < 1 || rating > 5)
                return new GeneralResponse(false, null, "Rating must be between 1 and 5.", null);

            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
                return new GeneralResponse(false, null, "User not found.", null);

            var restaurant = await _dbContext.Restaurants.FindAsync(restaurantId);
            if (restaurant == null)
                return new GeneralResponse(false, null, "Restaurant not found.", null);

            var hasReservation = await _dbContext.Reservations
                .AnyAsync(r => r.UserId == userId &&
                               r.Table.RestaurantId == restaurantId &&
                               (r.Status == ReservationStatus.Active || r.Status == ReservationStatus.NoActive));

            if (!hasReservation)
                return new GeneralResponse(false, null, "User has no reservations for this restaurant.", null);

            var review = new Review
            {
                UserId = userId,
                RestaurantId = restaurantId,
                Comment = comment,
                Rating = rating,
                CreatedAt = DateTime.UtcNow,
            };

            try
            {
                await _dbContext.Reviews.AddAsync(review);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new GeneralResponse(false, null, "An error occurred while saving the review.", ex.Message);
            }

            return new GeneralResponse(true, "Review added successfully.", null, review);
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

            var restaurant = await _dbContext.Restaurants.FirstOrDefaultAsync(r => r.Id == restaurantId);
            if (restaurant == null)
                return new GeneralResponse(false, null, "Restaurant not found.", null);

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (string.IsNullOrEmpty(userId) || user == null)
                return new GeneralResponse(false, null, "User is not authenticated.", null);

            var existingFavourite = await _dbContext.Favourites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.RestaurantId == restaurantId);

            if (existingFavourite == null)
                return new GeneralResponse(false, null, "This restaurant is not in your favourites.", null);

            _dbContext.Favourites.Remove(existingFavourite);
            await _dbContext.SaveChangesAsync();

            return new GeneralResponse(true, "Restaurant removed from favourites successfully.", null, null);
        }


        #endregion

    }
}
