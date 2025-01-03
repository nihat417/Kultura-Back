using Kultura.Application.Dto;
using Kultura.Application.Dto.RestaurntDtos;
using Kultura.Application.Model;
using Kultura.Application.Repository.Abstract;
using Kultura.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Kultura.Presentation.Areas.Restaurant.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RestaurantController(IUnitOfWork _unitOfWork) : ControllerBase
    {
        #region auth

        [HttpPost("Login")]
        public async Task<IActionResult> LoginRestaurant(RestaurantLoginDto loginDTO)
        {
            var response = await _unitOfWork.RestaurantService.LoginRestaurant(loginDTO);
            return Ok(response);
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RestaurantRegisterDto registerDTO)
        {
            var response = await _unitOfWork.RestaurantService.RegisterRestaurant(registerDTO);
            if (response != null && response.Success == true)
            {
                var user = await _unitOfWork.RestaurantService.FindEmailRestaurant(registerDTO.Email);
                if (user != null)
                {
                    var tokenResponse = await _unitOfWork.RestaurantService.GenerateEmailConfirmToken(registerDTO.Email);
                    if (tokenResponse.Success)
                    {
                        var token = tokenResponse.Data as string;
                        var confirmLink = Url.Action(
                            "ConfirmEmail",
                            "Restaurant",
                            new { token, email = registerDTO.Email },
                            Request.Scheme);

                        var message = new Message(new[] { registerDTO.Email }, "Confirmation Email Link", confirmLink!);
                        _unitOfWork.EmailService.SendEmail(message);

                        return Ok(response);
                    }
                }
                return BadRequest("Failed to generate email confirmation token");
            }
            return BadRequest(response);
        }

        #endregion

        #region EmailConfirm

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string token)
        {
            var email = GetEmailFromToken(token);

            var restaurant = await _unitOfWork.RestaurantService.GetByEmailAsync(email);

            if (restaurant == null) return BadRequest("Restaurant not found.");

            var isValidToken = _unitOfWork.JwtTokenService.ValidateEmailConfirmationTokenAsync(token, restaurant);

            if (!isValidToken) return BadRequest("Invalid token.");

            restaurant.EmailConfirmed = true;
            await _unitOfWork.RestaurantService.UpdateAsync(restaurant);

            return Ok("Email confirmed successfully.");
        }

        private string GetEmailFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
            return jsonToken?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        }

        #endregion

        #region get operations

        [HttpGet("getRestaurantById/{id}")]
        public async Task<IActionResult> GetRestaurantById(string id)
        {
            var response = await _unitOfWork.RestaurantService.GetRestaurantById(id);
            if (response.Success) return Ok(response);

            return NotFound(response);
        }

        [HttpGet("getRestaurantByEmail/{email}")]
        public async Task<IActionResult> GetRestaurantByEmail(string email)
        {
            var response = await _unitOfWork.RestaurantService.GetRestaurantByEmail(email);
            if (response.Success) return Ok(response);

            return NotFound(response);
        }

        [HttpGet("getfloorsId-restaurant/{restaurantId}")]
        public async Task<IActionResult> GetAllFloorsIdRestaurant(string restaurantId)
        {
            var response = await _unitOfWork.RestaurantService.GetAllFloorId(restaurantId);
            if (response.Success) return Ok(response);
            return NotFound(response);
        }

        [HttpGet("getTableId-floors")]
        public async Task<IActionResult> GetAllTablesIdFloor([FromQuery] string restaurantId, [FromQuery] string floorId)
        {
            var response = await _unitOfWork.RestaurantService.GetAllTablesId(restaurantId,floorId);
            if (response.Success) return Ok(response);
            return NotFound(response);
        }

        [HttpGet("get-slot/{tableId}")]
        public async Task<IActionResult> GetSlotById(string tableId)
        {
            var response = await _unitOfWork.RestaurantService.GetTableSlotsIdAsync(tableId);

            if (!response.Success) return BadRequest(response);

            return Ok(response);
        }


        #endregion

        #region post operations

        [HttpPost("add-floor")]
        public async Task<IActionResult> AddFloor([FromBody] FloorDto floorDto)
        {
            if (floorDto == null) return BadRequest("Floor DTO is null.");

            var response = await _unitOfWork.RestaurantService.AddFloor(floorDto);
            if (response.Success) return Ok(response);

            return BadRequest(response);
        }

        [HttpPost("add-Table")]
        public async Task<IActionResult> AddTable([FromBody] TableDto tableDto)
        {
            if (tableDto == null) return BadRequest("Floor DTO is null.");

            var response = await _unitOfWork.RestaurantService.AddTable(tableDto);
            if (response.Success) return Ok(response);

            return BadRequest(response);
        }

        [HttpPost("add-slot")]
        public async Task<IActionResult> AddSlotTable([FromBody] SlotDto slotDto)
        {
            if (slotDto == null) return BadRequest("Floor DTO is null.");

            var response = await _unitOfWork.RestaurantService.AddSlotTable(slotDto);
            if (response.Success) return Ok(response);

            return BadRequest(response);
        }

        [HttpPost("add-slot-to-floor-tables")]
        public async Task<IActionResult> AddSlotToFloor([FromBody] AddSlotRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var response = await _unitOfWork.RestaurantService.AddSlotToFloorTables(request);
            if (!response.Success) return BadRequest(response);

            return Ok(response);
        }


        [HttpPost("{reservationId}/complete")]
        public async Task<IActionResult> CompleteReservation(string reservationId)
        {
            if (reservationId == null) return BadRequest();
            var response = await _unitOfWork.RestaurantService.CompleteReservationAsync(reservationId);
            if (!response.Success)return BadRequest(response);

            return Ok(response);
        }

        [HttpPost("{restaurantId}/socials")]
        public async Task<IActionResult> AddSocialLink(string restaurantId, [FromBody] AddSocialLinkRequest request)
        {
            if (string.IsNullOrWhiteSpace(restaurantId))
                return BadRequest(new { message = "Restaurant ID is required." });

            if (!Enum.TryParse<SocialType>(request.SocialType, true, out var socialType))
                return BadRequest(new { message = "Invalid social type." });

            var response = await _unitOfWork.RestaurantService.AddSocialsAsync(restaurantId, request.Url, socialType);
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpPost("AddRestaurantMainPhoto")]
        public async Task<IActionResult> AddMainPhotoRestaurant([FromQuery] RestaurantMainPhotoDto photoDto)
        {
            if (!ModelState.IsValid) return BadRequest("Invalid input data.");
            var response = await _unitOfWork.RestaurantService.AddRestaurantMainPhoto(photoDto);
            if (!response.Success) return BadRequest(response);
            return Ok(response);
        }

        #endregion

        #region delete

        [HttpDelete("delete-slots-from-floor-tables")]
        public async Task<IActionResult> DeleteSlotsFromFloor([FromBody] DeleteSlotRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var response = await _unitOfWork.RestaurantService.DeleteSlotsFromFloorTables(request);
            if (!response.Success) return BadRequest(response);

            return Ok(response);
        }

        [HttpDelete]
        [Route("DeleteFloor")]
        public async Task<IActionResult> DeleteFloor([FromBody] FloorDto floordto)
        {
            if (floordto == null) return BadRequest(new { message = "FloorDto cannot be null" });

            var response = await _unitOfWork.RestaurantService.DeleteFloor(floordto);

            if (!response.Success)return BadRequest(new { message = response.Message });

            return Ok(new { message = response.Message });
        }

        [HttpDelete]
        [Route("DeleteTable")]
        public async Task<IActionResult> DeleteTable(string tableId, string restaurantId)
        {
            var response = await _unitOfWork.RestaurantService.DeleteTable(tableId, restaurantId);

            if (!response.Success)return BadRequest(new { message = response.Message });

            return Ok(new { message = response.Message });
        }

        [HttpDelete("delete-slot/{slotId}")]
        public async Task<IActionResult> DeleteSlot(string slotId)
        {
            var response = await _unitOfWork.RestaurantService.DeleteSlotAsync(slotId);

            if (!response.Success) return BadRequest(new { message = response.Message });

            return Ok(new { message = response.Message });
        }


        #endregion

    }
}
