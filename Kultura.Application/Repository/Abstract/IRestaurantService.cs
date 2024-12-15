using Kultura.Application.Dto;
using Kultura.Application.Dto.RestaurntDtos;
using Kultura.Domain.Entities;
using static Kultura.Application.Model.Responses.ServiceResponses;

namespace Kultura.Application.Repository.Abstract
{
    public interface IRestaurantService
    {
        Task<LoginResponse> LoginRestaurant(RestaurantLoginDto restaurantLogin);
        Task<GeneralResponse> RegisterRestaurant(RestaurantRegisterDto restaurantRegister);


        #region general services

        //get
        Task<GeneralResponse> GetRestaurantById(string id);
        Task<GeneralResponse> GetRestaurantByEmail(string email);
        Task<GeneralResponse> GetAllFloorId(string restaurantId);
        Task<GeneralResponse> GetAllTablesId(string restaurantId, string floorId);
        Task<GeneralResponse> GetTableSlotsIdAsync(string tableId);
        Task<GeneralResponse> CompleteReservationAsync(string reservationId);


        //post
        Task<GeneralResponse> AddFloor(FloorDto floor);
        Task<GeneralResponse> AddTable(TableDto tableDto);
        Task<GeneralResponse> AddSlotTable(SlotDto slotDto);
        Task<GeneralResponse> AddSlotToFloorTables(AddSlotRequest request);

        //delete
        Task<GeneralResponse> DeleteFloor(FloorDto floor);
        Task<GeneralResponse> DeleteTable(string tableId, string restaurantId);
        Task<GeneralResponse> DeleteSlotAsync(string slotId);
        Task<GeneralResponse> DeleteSlotsFromFloorTables(DeleteSlotRequest request);

        #endregion

        //operation services
        Task<GeneralResponse> FindEmailRestaurant(string email);
        Task<Restaurant> GetByEmailAsync(string email);


        //token services
        Task<GeneralResponse> GenerateEmailConfirmToken(string email);
        Task<GeneralResponse> ConfirmEmail(string token, string email);


        Task UpdateAsync(Restaurant restaurant);
    }
}
