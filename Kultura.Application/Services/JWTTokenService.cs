using Microsoft.Extensions.Configuration;
using Kultura.Application.Model;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Kultura.Application.Services
{
    public class JwtTokenService(IConfiguration _config)
    {
        public (string, string) CreateToken(UserSession user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var userClaims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id !),
                new Claim(ClaimTypes.Name, user.Name !),
                new Claim(ClaimTypes.Email, user.Email !),
                new Claim(ClaimTypes.Role, user.Role !)
            };
            var accessToken = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: userClaims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials
            );

            var refreshToken = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials
            );


            return (new JwtSecurityTokenHandler().WriteToken(accessToken), new JwtSecurityTokenHandler().WriteToken(refreshToken));
        }

        public string GetUserIdFromToken(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
                throw new ArgumentNullException(nameof(accessToken), "Access token cannot be null or empty.");

            var handler = new JwtSecurityTokenHandler();
            try
            {
                var token = handler.ReadJwtToken(accessToken);

                var userIdClaim = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                    throw new Exception("User ID claim not found in token.");

                return userIdClaim.Value;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error parsing token: {ex.Message}", ex);
            }
        }

        /*public string RefreshAccessToken(string refreshToken, UserSession user)
        {
            try
            {
                var principal = GetPrincipalFromExpiredToken(refreshToken);

                if (principal == null) throw new SecurityTokenException("Invalid refresh token");

                var userId = user.Id!;

                var userClaims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id !),
                    new Claim(ClaimTypes.Name, user.Name !),
                    new Claim(ClaimTypes.Email, user.Email !),
                    new Claim(ClaimTypes.Role, user.Role !)
                };

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var accessToken = new JwtSecurityToken(
                    issuer: _config["Jwt:Issuer"],
                    audience: _config["Jwt:Audience"],
                    claims: userClaims,
                    expires: DateTime.Now.AddMinutes(Convert.ToDouble(_config["Jwt:AccessTokenExpirationMinutes"])),
                    signingCredentials: credentials
                );

                return new JwtSecurityTokenHandler().WriteToken(accessToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error refreshing access token: {ex.Message}");
                return null!;
            }
        }


        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false 
            };

            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, validationParameters, out securityToken);
            return principal;
        }*/

    }

}
