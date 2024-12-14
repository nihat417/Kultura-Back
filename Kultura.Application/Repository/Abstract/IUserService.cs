using Kultura.Application.Dto;
using static Kultura.Application.Model.Responses.ServiceResponses;

namespace Kultura.Application.Repository.Abstract
{
    public interface IUserService
    {
        Task<GeneralResponse> AddReservationAsync(ReservationDto reservationDto);
        Task<GeneralResponse> DeleteReservationAsync(string reservationId);
        Task<GeneralResponse> SearchRestaurantsAsync(string search);
        Task<GeneralResponse> AddFavourites(string userId,string restaurantId);
        Task<GeneralResponse> DeleteFavourite(string userId, string restaurantId);
        Task<GeneralResponse> GetAllFavourites(string userId);
    }
}
