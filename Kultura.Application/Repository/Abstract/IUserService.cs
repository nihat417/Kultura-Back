using Kultura.Application.Dto;
using static Kultura.Application.Model.Responses.ServiceResponses;

namespace Kultura.Application.Repository.Abstract
{
    public interface IUserService
    {
        Task<GeneralResponse> GetAllFavourites(string userId);
        Task<GeneralResponse> SearchRestaurantsAsync(string search);
        Task<GeneralResponse> GetReserveHistoryAsync(string userId);

        Task<GeneralResponse> AddReservationAsync(ReservationDto reservationDto);
        Task<GeneralResponse> AddFavourites(string userId,string restaurantId);
        Task<GeneralResponse> CancelReservationAsync(string reservationId);
        Task<GeneralResponse> AddReviewAsync(string userId, string restaurantId, string comment, int rating);

        Task<GeneralResponse> DeleteReservationAsync(string reservationId);
        Task<GeneralResponse> DeleteFavourite(string userId, string restaurantId);
        Task<GeneralResponse> DeleteReviewAsync(string reviewId, string userId);


        Task<GeneralResponse> UpdateUserProfileAsync(string userId, string fullName, string? country, int age, string? imageUrl);
    }
}
