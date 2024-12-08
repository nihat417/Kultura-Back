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
            {
                return Unauthorized(new { Message = "Token is missing or invalid." });
            }

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
    }
}
