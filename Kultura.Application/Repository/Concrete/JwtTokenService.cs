using Kultura.Application.Model;
using Kultura.Application.Repository.Abstract;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Kultura.Application.Repository.Concrete
{
    public class JwtTokenService(IConfiguration _configuration) : IJwtTokenService
    {
        public (string, string) CreateToken(UserSession user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var userClaims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id !),
                new Claim(ClaimTypes.Name, user.FullName !),
                new Claim(ClaimTypes.Email, user.Email !),
                new Claim(ClaimTypes.Role, user.Role !)
            };
            var accessToken = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: userClaims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credentials
            );

            var refreshToken = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
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

                if (userIdClaim == null) throw new Exception("User ID claim not found in token.");

                return userIdClaim.Value;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error parsing token: {ex.Message}", ex);
            }
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync<T>(T entity) where T : class
        {
            var emailProperty = typeof(T).GetProperty("Email");
            var idProperty = typeof(T).GetProperty("Id");

            if (emailProperty == null || idProperty == null)
                throw new ArgumentException("The provided entity does not have required properties 'Email' and 'Id'.");

            var email = emailProperty.GetValue(entity)?.ToString();
            var id = idProperty.GetValue(entity)?.ToString();

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentNullException(nameof(email), "Email cannot be null or empty.");
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id), "ID cannot be null or empty.");

            var secret = _configuration["Jwt:Key"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience))
                throw new InvalidOperationException("JWT configuration is missing or invalid.");

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, email),
        new Claim(ClaimTypes.NameIdentifier, id)
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public bool ValidateEmailConfirmationTokenAsync<T>(string token, T entity) where T : class
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity), "Entity cannot be null.");

                var emailProperty = typeof(T).GetProperty("Email");
                if (emailProperty == null)
                    throw new InvalidOperationException("The entity does not contain an 'Email' property.");

                var emailValue = emailProperty.GetValue(entity)?.ToString();
                if (string.IsNullOrWhiteSpace(emailValue))
                    throw new InvalidOperationException("The 'Email' property value cannot be null or empty.");

                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
                var emailClaim = jsonToken?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

                return emailClaim == emailValue;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
