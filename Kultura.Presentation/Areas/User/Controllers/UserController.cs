using Kultura.Application.Dto;
using Kultura.Application.Repository.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kultura.Presentation.Areas.User.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(IUnitOfWork _unitOfWork) : ControllerBase
    {
        [HttpGet("getId")]
        public IActionResult GetUserInfo()
        {
            var authorizationHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
                return Unauthorized(new { Message = "Token is missing or invalid." });

            var token = authorizationHeader.Substring("Bearer ".Length).Trim();

            try
            {
                var userId = _unitOfWork.JwtTokenService.GetUserIdFromToken(token);
                return Ok(new { UserId = userId });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { Message = $"Error extracting user ID: {ex.Message}" });
            }
        }

        #region get

        [HttpGet("getAll-Favourites")]
        public async Task<IActionResult> GetAllFavourites([FromQuery] string userId)
        {
            var result = await _unitOfWork.UserService.GetAllFavourites(userId);

            if (!result.Success) return NotFound();

            return Ok(result);
        }

        [HttpGet("getAll-ReserveHistory/{userId}")]
        public async Task<IActionResult> GetRserveHistory([FromQuery] string userId)
        {
            var result = await _unitOfWork.UserService.GetReserveHistoryAsync(userId);
            if (!result.Success) return NotFound();
            return Ok(result);
        }

        #endregion

        #region post

        [HttpPost("add-reservation")]
        public async Task<IActionResult> AddReservation([FromBody] ReservationDto reservationDto)
        {
            if (reservationDto == null)return BadRequest();

            var response = await _unitOfWork.UserService.AddReservationAsync(reservationDto);
            if (!response.Success) return BadRequest(response);

            return Ok(response);
        }

        [HttpPost("{reservationId}/cancel")]
        public async Task<IActionResult> CancelReservation(string reservationId)
        {
            if (string.IsNullOrEmpty(reservationId)) return BadRequest();

            var response = await _unitOfWork.UserService.CancelReservationAsync(reservationId);
            if (!response.Success) return BadRequest(response);

            return Ok(response);
        }

        [HttpPost("search-restaurants")]
        public async Task<IActionResult> SearchRestaurants([FromBody] string search)
        {
            if (string.IsNullOrEmpty(search)) return BadRequest();

            var response = await _unitOfWork.UserService.SearchRestaurantsAsync(search);
            if (!response.Success) return NotFound(response);

            return Ok(response);
        }

        [HttpPost("add-favourites")]
        public async Task<IActionResult> AddToFavourites([FromQuery] string userId,string restaurantId)
        {
            if (userId == null || restaurantId == null) return BadRequest();

            var response = await _unitOfWork.UserService.AddFavourites(userId,restaurantId);
            if (!response.Success) return BadRequest(response);

            return Ok(response);
        }

        [HttpPost("add-review")]
        public async Task<IActionResult> AddReview([FromBody] AddReviewDto addReviewDto)
        {
            if (!ModelState.IsValid) return BadRequest("Invalid input data.");

            var response = await _unitOfWork.UserService.AddReviewAsync(addReviewDto.UserId, addReviewDto.RestaurantId, addReviewDto.Comment, addReviewDto.Rating);
            if (!response.Success) return BadRequest(response);

            return Ok(response);
        }

        #endregion

        #region delete

        [HttpDelete("delete-reservation/{reservationId}")]
        public async Task<IActionResult> DeleteReservation(string reservationId)
        {
            if (string.IsNullOrEmpty(reservationId)) return BadRequest();

            var response = await _unitOfWork.UserService.DeleteReservationAsync(reservationId);
            if (!response.Success) return NotFound(response);

            return Ok(response);
        }

        [HttpDelete("delete-favourite")]
        public async Task<IActionResult> DeleteFavourite([FromQuery] string userId, [FromQuery] string restaurantId)
        {
            var result = await _unitOfWork.UserService.DeleteFavourite(userId, restaurantId);

            if (!result.Success) return BadRequest(new { Message = result.Message });

            return Ok(result);
        }

        [HttpDelete("delete-myreview")]
        public async Task<IActionResult> DeleteMyreView([FromQuery] string reviewId, [FromQuery] string userId)
        {
            var result = await _unitOfWork.UserService.DeleteReviewAsync(reviewId, userId);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        #endregion


        #region put

        [HttpPut("/UpdateProfile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto request)
        {
            if (request == null)
                return BadRequest(new { message = "Invalid request data." });

            var response = await _unitOfWork.UserService.UpdateUserProfileAsync(
                request.UserId,
                request.FullName,
                request.Country,
                request.Age,
                request.ImageUrl
            );

            if (response.Success) return Ok(new { message = response.Message, data = response.Data });

            return BadRequest(new { message = response.Message, error = response.ErrorMessage });
        }


        #endregion
    }
}
