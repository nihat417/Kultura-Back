﻿using Kultura.Application.Dto.AuthDto;
using Kultura.Application.Dto.RestaurntDtos;
using Kultura.Application.Model;
using Kultura.Application.Repository.Abstract;
using Kultura.Application.Repository.Concrete;
using Microsoft.AspNetCore.Http;
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




    }
}