using Kultura.Application.Dto.RestaurntDtos;
using Kultura.Application.Model;
using Kultura.Application.Repository.Abstract;
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

        #endregion


        #region delete

        [HttpDelete]
        [Route("DeleteFloor")]
        public async Task<IActionResult> DeleteFloor([FromBody] FloorDto floordto)
        {
            if (floordto == null)
                return BadRequest(new { message = "FloorDto cannot be null" });

            var response = await _unitOfWork.RestaurantService.DeleteFloor(floordto);

            if (!response.Success)return BadRequest(new { message = response.Message });

            return Ok(new { message = response.Message });
        }

        #endregion

    }
}
