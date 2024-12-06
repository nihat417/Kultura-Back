using Kultura.Application.Dto.AuthDto;
using Kultura.Application.Dto.RestaurntDtos;
using Kultura.Application.Model;
using Kultura.Application.Repository.Abstract;
using Kultura.Application.Repository.Concrete;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kultura.Presentation.Areas.Restaurant.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RestaurantController(IUnitOfWork _unitOfWork) : ControllerBase
    {
        #region auth

        [HttpPost("LoginRestaurant")]
        public async Task<IActionResult> LoginRestaurant(RestaurantLoginDto loginDTO)
        {
            var response = await _unitOfWork.RestaurantService.LoginRestaurant(loginDTO);
            return Ok(response);
        }

        [HttpPost("RegisterRestaurant")]
        public async Task<IActionResult> Register(RestaurantRegisterDto registerDTO)
        {
            var response = await _unitOfWork.RestaurantService.RegisterRestaurant(registerDTO);
            if (response != null && response.Success == true)
            {
                var user = await _unitOfWork.RestaurantService.FindEmailRestaurant(registerDTO.Email);
                if (user != null)
                {
                    //var token = await _unitOfWork.RestaurantService.GenerateEmailConfirmToken(registerDTO.Email);
                    //if (!string.IsNullOrEmpty(token))
                    //{
                    //    var confirmLink = Url.Action(
                    //        "ConfirmEmail",
                    //        "Auth",
                    //        new { token, email = registerDTO.Email },
                    //        Request.Scheme);

                    //    var message = new Message(new[] { registerDTO.Email }, "Confirmation Email Link", confirmLink!);
                    //    _unitOfWork.EmailService.SendEmail(message);
                    //    return Ok(response);
                    //}
                    //return BadRequest("Failed to generate email confirmation token");
                }
                return BadRequest("User not found");
            }
            return BadRequest(response);
        }

        #endregion

        #region EmailConfirm

        //[HttpGet("ConfirmEmail")]
        //public async Task<IActionResult> ConfirmEmail(string token, string email)
        //{
        //    if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(email))
        //        return BadRequest("Invalid token or email");

        //    var user = await _unitOfWork.RestaurantService.FindByEmailAsync(email);
        //    if (user == null)
        //        return NotFound("User not found");

        //    var result = await _unitOfWork.UserService.ConfirmEmailAsync(user, token);
        //    if (!result.Succeeded)
        //        return BadRequest("Failed to confirm email");

        //    var emailConfirmed = await _unitOfWork.RestaurantService.ConfirmRestaurantEmail(email);
        //    if (!emailConfirmed)
        //        return BadRequest("Failed to update restaurant email confirmation");

        //    return Ok("Email confirmed successfully");
        //}

        #endregion




    }
}
