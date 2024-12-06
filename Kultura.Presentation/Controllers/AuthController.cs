using Kultura.Application.Dto.AuthDto;
using Kultura.Application.Model;
using Kultura.Application.Repository.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace Kultura.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IUnitOfWork _unitOfWork) : ControllerBase
    {
        #region AuthPost

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDto loginDTO)
        {
            var response = await _unitOfWork.AuthService.Login(loginDTO);
            return Ok(response);
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDto registerDTO)
        {
            var response = await _unitOfWork.AuthService.Register(registerDTO);
            if (response != null && response.Success == true)
            {
                var user = await _unitOfWork.UserManager.FindByEmailAsync(registerDTO.Email);
                if (user != null)
                {
                    var token = await _unitOfWork.UserManager.GenerateEmailConfirmationTokenAsync(user!);
                    var confirmLink = Url.Action("ConfirmEmail", "Auth", new { token, email = registerDTO.Email }, Request.Scheme);
                    var message = new Message(new string[] { registerDTO.Email }, "Confirmation Email Link", confirmLink!);
                    _unitOfWork.EmailService.SendEmail(message);
                    return Ok(response);
                }
                return BadRequest();
            }
            return BadRequest(response);
        }

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _unitOfWork.UserManager.FindByEmailAsync(email);
            if (user == null || !(await _unitOfWork.UserManager.IsEmailConfirmedAsync(user)))
                return BadRequest("User not found or email is not confirmed.");

            var token = await _unitOfWork.UserManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = Url.Action("ResetPassword", "Auth", new { token, email }, Request.Scheme);
            var message = new Message(new string[] { email }, "Reset Password Link", resetLink!);
            _unitOfWork.EmailService.SendEmail(message);

            return Ok("Password reset link has been sent to your email.");
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(string token, string email, string newPassword)
        {
            var user = await _unitOfWork.UserManager.FindByEmailAsync(email);
            if (user == null) return BadRequest("User not found.");

            var result = await _unitOfWork.UserManager.ResetPasswordAsync(user, token, newPassword);
            if (result.Succeeded) return Ok("Password has been reset successfully.");
            else return BadRequest("Failed to reset password.");
        }

        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword(string email, string currentPassword, string newPassword)
        {
            var user = await _unitOfWork.UserManager.FindByEmailAsync(email);
            if (user == null) return BadRequest("User not found.");

            var passwordCheckResult = await _unitOfWork.UserManager.CheckPasswordAsync(user, currentPassword);
            if (!passwordCheckResult) return BadRequest("Current password is incorrect.");

            var changePasswordResult = await _unitOfWork.UserManager.ChangePasswordAsync(user, currentPassword, newPassword);

            if (changePasswordResult.Succeeded) return Ok("Password has been changed successfully.");
            else return BadRequest("Failed to change password.");
        }


        #endregion


        #region EmailConfirm

        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            try
            {
                var user = await _unitOfWork.UserManager.FindByEmailAsync(email);
                if (user == null) return NotFound();

                var result = await _unitOfWork.UserManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded) return Ok();
                else return BadRequest("Failed to confirm email");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        #endregion

    }
}
