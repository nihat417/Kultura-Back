﻿using Kultura.Application.Dto;
using Kultura.Application.Dto.RestaurntDtos;
using Kultura.Application.Model;
using Kultura.Application.Repository.Abstract;
using Kultura.Domain.Entities;
using Kultura.Domain.Enums;
using Kultura.Persistence.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static Kultura.Application.Model.Responses.ServiceResponses;
using static Kultura.Application.Services.UploadFileHelper;

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
                var restaurantData = await _dbContext.Restaurants
                    .AsNoTracking()
                    .Include(r => r.Reviews)
                    .Where(r => r.Id == id)
                    .Select(r => new
                    {
                        r.Name,
                        r.Location,
                        r.Description,
                        r.MainPhoto,
                        r.Email,
                        r.PhoneNumber,
                        r.Cuisines,
                        r.EmailConfirmed,
                        r.Photos,
                        r.OpeningTime,
                        r.ClosingTime,
                        r.MinPrice,
                        r.MaxPrice,
                        AverageRating = r.Reviews.Any()
                    ? Math.Round(r.Reviews.Average(review => review.Rating), 1)
                    : 0
                    })
                    .FirstOrDefaultAsync();

                if (restaurantData == null)
                    return new GeneralResponse(false, null, "Restaurant not found", null);

                return new GeneralResponse(true, "Restaurant found", null, restaurantData);
            }
            catch (Exception ex)
            {
                return new GeneralResponse(false, null, $"Error retrieving restaurant: {ex.Message}", null);
            }
        }

        public async Task<GeneralResponse> GetRestaurantByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return new GeneralResponse(false, null, "Email is null or empty", null);

            try
            {
                var restaurantData = await _dbContext.Restaurants
                    .AsNoTracking()
                    .Include(r => r.Reviews)
                    .Where(r => r.Email == email)
                    .Select(r => new
                    {
                        r.Name,
                        r.Location,
                        r.Description,
                        r.MainPhoto,
                        r.Email,
                        r.PhoneNumber,
                        r.Cuisines,
                        r.OpeningTime,
                        r.ClosingTime,
                        r.MinPrice,
                        r.MaxPrice,
                        AverageRating = r.Reviews.Any()
                            ? Math.Round(r.Reviews.Average(review => review.Rating), 1)
                            : 0
                    })
                    .FirstOrDefaultAsync();

                if (restaurantData == null)
                    return new GeneralResponse(false, null, "Restaurant not found", null);

                return new GeneralResponse(true, "Restaurant found", null, restaurantData);
            }
            catch (Exception ex)
            {
                return new GeneralResponse(false, null, $"Error retrieving restaurant: {ex.Message}", null);
            }
        }


        public async Task<GeneralResponse> GetAllFloorId(string restaurantId)
        {
            var restaurant = await _dbContext.Restaurants.FirstOrDefaultAsync(r => r.Id == restaurantId);

            if (restaurant == null) return new GeneralResponse(false, null, "Restaurant not found", null);

            try
            {
                var floorIds = await _dbContext.Floors
                    .Where(f => f.RestaurantId == restaurantId)
                    .Select(f => f.Id)
                    .ToListAsync();

                if (!floorIds.Any()) return new GeneralResponse(false, null, "No floors found for this restaurant", null);

                return new GeneralResponse(true, "Floors retrieved successfully", null, floorIds);
            }
            catch (Exception ex)
            {
                return new GeneralResponse(false, null, ex.Message, null);
            }
        }

        public async Task<GeneralResponse> GetAllTablesId(string restaurantId,string floorId)
        {
            var restaurant = await _dbContext.Restaurants.FirstOrDefaultAsync(r => r.Id == restaurantId);

            if (restaurant == null) return new GeneralResponse(false, null, "Restaurant not found", null);

            var floor = await _dbContext.Floors.FirstOrDefaultAsync(f => f.Id == floorId && f.RestaurantId == restaurantId);

            if (floor == null) return new GeneralResponse(false, null, "Floor not found for the specified restaurant", null);

            try
            {
                var tableIds = await _dbContext.Tables
                    .Where(t => t.RestaurantId == restaurantId && t.FloorNumber == floor.Number)
                    .Select(t => t.Id)
                    .ToListAsync();

                if (!tableIds.Any()) return new GeneralResponse(false, null, "No tables found for the specified floor", null);

                return new GeneralResponse(true, "Tables retrieved successfully", null, tableIds);
            }
            catch (Exception ex)
            {
                return new GeneralResponse(false, null, ex.Message, null);
            }
        }

        public async Task<GeneralResponse> GetTableSlotsIdAsync(string tableId)
        {
            if (string.IsNullOrEmpty(tableId))return new GeneralResponse(false, null, "Table ID cannot be null or empty.", null);

            var table = await _dbContext.Tables.FirstOrDefaultAsync(t => t.Id == tableId);

            if (table == null) return new GeneralResponse(false, null, "table not found.", null);

            try
            {
                var tableSlots = await _dbContext.ReservationSlots
                    .Where(rs => rs.TableId == tableId)
                    .Select(rs => new
                    {
                        rs.Id,
                        StartTime = rs.StartTime.ToString(@"hh\:mm"), 
                        EndTime = rs.EndTime.ToString(@"hh\:mm"),     
                        rs.IsReserved
                    })
                    .ToListAsync();

                return new GeneralResponse(true, "Table slots retrieved successfully.", null, tableSlots);
            }
            catch (Exception ex)
            {
                return new GeneralResponse(false, null, $"Error retrieving table slots: {ex.Message}", null);
            }
        }

        public async Task<GeneralResponse> CompleteReservationAsync(string reservationId)
        {
            var reservation = await _dbContext.Reservations.FindAsync(reservationId);
            if (reservation == null)
                return new GeneralResponse(false, null, "Reservation not found.", null);

            if (reservation.Status != ReservationStatus.Active)
                return new GeneralResponse(false, null, "Only active reservations can be completed.", null);

            reservation.Status = ReservationStatus.NoActive;

            await _dbContext.SaveChangesAsync();

            return new GeneralResponse(true, "Reservation completed successfully.", null, reservation);
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

        private bool AreTablesOverlapping(Table existingTable, TableDto newTable)
        {
            var distance = Math.Sqrt(
                Math.Pow(existingTable.X - newTable.X, 2) +
                Math.Pow(existingTable.Y - newTable.Y, 2)
            );
            return distance < (existingTable.Radius + newTable.Radius);
        }

        public async Task<GeneralResponse> AddTable(TableDto tableDto)
        {
            if (tableDto == null) return new GeneralResponse(false, null, "Table DTO is null", null);

            var floor = await _dbContext.Floors.FirstOrDefaultAsync(f => f.Number == tableDto.FloorNumber);
            if (floor == null) return new GeneralResponse(false, null, "Floor not found", null);

            var existingTables = await _dbContext.Tables
                .Where(t => t.FloorNumber == tableDto.FloorNumber && t.RestaurantId == tableDto.RestaurantId)
                .ToListAsync();

            if (existingTables.Any(t => AreTablesOverlapping(t, tableDto))) 
                return new GeneralResponse(false, null, "New table overlaps with existing ones.", null);

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

        public async Task<GeneralResponse> AddSlotTable(SlotDto slotDto)
        {
            if (slotDto.startTime >= slotDto.endTime) return new GeneralResponse(false,null,"Start time must be earlier than end time.",null);

            var existingSlot = await _dbContext.ReservationSlots
                        .FirstOrDefaultAsync(slot =>slot.TableId == slotDto.tableId &&
                        ((slotDto.startTime >= slot.StartTime && slotDto.startTime < slot.EndTime) ||
                         (slotDto.endTime > slot.StartTime && slotDto.endTime <= slot.EndTime)));


            if (existingSlot != null) return new GeneralResponse (false,null,"A reservation slot already exists for this table at the specified time.",null);

            var newSlot = new ReservationSlot
            {
                TableId = slotDto.tableId,
                StartTime = slotDto.startTime,
                EndTime = slotDto.endTime,
                IsReserved = false
            };

            _dbContext.ReservationSlots.Add(newSlot);
            await _dbContext.SaveChangesAsync();


            return new GeneralResponse(true, "Reservation slot added successfully.", null, newSlot);
        }

        public async Task<GeneralResponse> AddSlotToFloorTables(AddSlotRequest request)
        {
            if (request.StartTime >= request.EndTime)
                return new GeneralResponse(false, null, "Start time must be earlier than end time.", null);

            var restaurant = await _dbContext.Restaurants.FirstOrDefaultAsync(r => r.Id == request.RestaurantId);
            if (restaurant == null)
                return new GeneralResponse(false, null, "Restaurant not found", null);

            var floor = await _dbContext.Floors.FirstOrDefaultAsync(f => f.Number == request.FloorNumber && f.RestaurantId == request.RestaurantId);
            if (floor == null)
                return new GeneralResponse(false, null, $"Floor with number {request.FloorNumber} not found for the specified restaurant.", null);

            var floorTables = await _dbContext.Tables
                .Where(t => t.RestaurantId == request.RestaurantId && t.FloorNumber == request.FloorNumber)
                .ToListAsync();

            if (!floorTables.Any())
                return new GeneralResponse(false, null, $"No tables found on floor {request.FloorNumber}.", null);

            var conflictingSlots = new List<string>();
            foreach (var table in floorTables)
            {
                var existingSlot = await _dbContext.ReservationSlots
                    .FirstOrDefaultAsync(slot =>
                        slot.TableId == table.Id &&
                        ((request.StartTime >= slot.StartTime && request.StartTime < slot.EndTime) ||
                         (request.EndTime > slot.StartTime && request.EndTime <= slot.EndTime)));

                if (existingSlot != null)
                {
                    conflictingSlots.Add($"Table ID {table.Id} already has a conflicting reservation slot.");
                    continue;
                }

                var newSlot = new ReservationSlot
                {
                    TableId = table.Id,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    IsReserved = false
                };

                _dbContext.ReservationSlots.Add(newSlot);
            }

            await _dbContext.SaveChangesAsync();

            if (conflictingSlots.Any())
            {
                return new GeneralResponse(
                    false,
                    null,
                    "Some slots were not added due to conflicts.",
                    new { Conflicts = conflictingSlots });
            }

            return new GeneralResponse(
                true,
                $"Reservation slots added successfully for all tables on floor {request.FloorNumber}.",
                null,
                null);
        }

        public async Task<GeneralResponse> DeleteSlotsFromFloorTables(DeleteSlotRequest request)
        {
            if (request.StartTime >= request.EndTime)
                return new GeneralResponse(false, null, "Start time must be earlier than end time.", null);

            var restaurant = await _dbContext.Restaurants.FirstOrDefaultAsync(r => r.Id == request.RestaurantId);
            if (restaurant == null)
                return new GeneralResponse(false, null, "Restaurant not found", null);

            var floor = await _dbContext.Floors.FirstOrDefaultAsync(f => f.Number == request.FloorNumber && f.RestaurantId == request.RestaurantId);
            if (floor == null)
                return new GeneralResponse(false, null, $"Floor with number {request.FloorNumber} not found for the specified restaurant.", null);

            var floorTables = await _dbContext.Tables
                .Where(t => t.RestaurantId == request.RestaurantId && t.FloorNumber == request.FloorNumber)
                .ToListAsync();

            if (!floorTables.Any())
                return new GeneralResponse(false, null, $"No tables found on floor {request.FloorNumber}.", null);

            var slotsToDelete = new List<ReservationSlot>();

            foreach (var table in floorTables)
            {
                var slots = await _dbContext.ReservationSlots
                    .Where(slot =>
                        slot.TableId == table.Id &&
                        slot.StartTime >= request.StartTime &&
                        slot.EndTime <= request.EndTime)
                    .ToListAsync();

                if (slots.Any())
                {
                    slotsToDelete.AddRange(slots);
                }
            }

            if (!slotsToDelete.Any())
            {
                return new GeneralResponse(
                    false,
                    null,
                    $"No reservation slots found on floor {request.FloorNumber} within the specified time range.",
                    null);
            }

            _dbContext.ReservationSlots.RemoveRange(slotsToDelete);
            await _dbContext.SaveChangesAsync();

            return new GeneralResponse(
                true,
                $"Successfully deleted {slotsToDelete.Count} reservation slots on floor {request.FloorNumber}.",
                null,
                null);
        }

        public async Task<GeneralResponse> AddSocialsAsync(string restaurantId, string url, SocialType socialType)
        {
            if (string.IsNullOrWhiteSpace(restaurantId))
                return new GeneralResponse(false, null, "Restaurant ID cannot be null or empty.", null);

            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                return new GeneralResponse(false, null, "Invalid URL format.", null);

            var restaurant = await _dbContext.Restaurants.Include(r => r.SocialLinks).FirstOrDefaultAsync(r => r.Id == restaurantId);
            if (restaurant == null)
                return new GeneralResponse(false, null, "Restaurant not found.", null);

            if (restaurant.SocialLinks.Any(sl => sl.Url == url))
                return new GeneralResponse(false, null, "This social link already exists.", null);

            var newSocialLink = new SocialLink
            {
                Url = url,
                Platform = socialType.ToString(),
            };
            restaurant.SocialLinks.Add(newSocialLink);

            try
            {
                _dbContext.Restaurants.Update(restaurant);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new GeneralResponse(false, null, "An error occurred while adding the social link.", ex.Message);
            }

            return new GeneralResponse(true, "Social link added successfully.", null, newSocialLink);
        }

        public async Task<GeneralResponse> AddRestaurantMainPhoto(RestaurantMainPhotoDto mainPhotoDto)
        {
            var restaurant = await _dbContext.Restaurants.FirstOrDefaultAsync(r => r.Id == mainPhotoDto.RestaurantId);

            if (restaurant == null)
                return new GeneralResponse(false, null, "restaurant not found", null);

            if (mainPhotoDto == null || mainPhotoDto.MainImage == null || string.IsNullOrEmpty(mainPhotoDto.RestaurantId))
                return new GeneralResponse(false, null, "Invalid restaurant photo DTO", null);

            try
            {
                if (!string.IsNullOrEmpty(restaurant.MainPhoto))
                {
                    string publicId = ExtractPublicIdFromUrl(restaurant.MainPhoto);
                    if (!string.IsNullOrEmpty(publicId))
                    {
                        Console.WriteLine("\n delete photo");
                        await CloudinaryService.DeleteFile(publicId);
                    }
                }

                restaurant.MainPhoto = (mainPhotoDto.MainImage != null)
                    ? await CloudinaryService.UploadFile(mainPhotoDto.MainImage, "restaurantmainphotos")
                    : "https://yandex.ru/images/search?img_url=http%3A%2F%2Fimages.hdqwalls.com%2Fdownload%2Fsunset-tree-red-ocean-sky-7w-3840x2160.jpg&lr=105888&pos=0&rpt=simage&serp_list_type=all&source=serp&text=image";


                _dbContext.Restaurants.Update(restaurant);
                await _dbContext.SaveChangesAsync();

                return new GeneralResponse(true, "restaurant photo updated successfully", null, restaurant);
            }
            catch (Exception ex)
            {
                return new GeneralResponse(false, null, $"Error uploading photo: {ex.Message}", null);
            }
        }

        public async Task<GeneralResponse> AddRestaurantPhotos(AddRestaurantPhotosDto photosDto)
        {
            var restaurant = await _dbContext.Restaurants.FirstOrDefaultAsync(r => r.Id == photosDto.RestaurantId);

            if (restaurant == null)
                return new GeneralResponse(false, null, "Restaurant not found", null);

            if (photosDto == null || photosDto.Images == null || !photosDto.Images.Any())
                return new GeneralResponse(false, null, "No photos provided", null);

            try
            {
                foreach (var image in photosDto.Images)
                {
                    var imageUrl = await CloudinaryService.UploadFile(image, "restaurantphotos");
                    restaurant.Photos.Add(imageUrl); 
                }

                _dbContext.Restaurants.Update(restaurant);
                await _dbContext.SaveChangesAsync();

                return new GeneralResponse(true, "Photos added successfully", null, restaurant);
            }
            catch (Exception ex)
            {
                return new GeneralResponse(false, null, $"Error uploading photos: {ex.Message}", null);
            }
        }


        private string ExtractPublicIdFromUrl(string imageUrl)
        {
            try
            {
                var uri = new Uri(imageUrl);
                var segments = uri.AbsolutePath.Split('/');
                var publicIdWithExtension = segments[^1];
                return publicIdWithExtension.Split('.')[0];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting public ID: {ex.Message}");
                return null;
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

        public async Task<GeneralResponse> DeleteSlotAsync(string slotId)
        {
            if (string.IsNullOrEmpty(slotId)) return new GeneralResponse(false, null, "Slot ID cannot be null or empty.", null);

            var slot = await _dbContext.ReservationSlots.FirstOrDefaultAsync(s => s.Id == slotId);

            if (slot == null) return new GeneralResponse(false, null, "Slot not found.", null);

            _dbContext.ReservationSlots.Remove(slot);
            await _dbContext.SaveChangesAsync();

            return new GeneralResponse(true, "Reservation slot deleted successfully.", null, null);
        }


        #endregion


        public async Task UpdateAsync(Restaurant restaurant)
        {
            _dbContext.Restaurants.Update(restaurant);
            await _dbContext.SaveChangesAsync();
        }
    }
}
