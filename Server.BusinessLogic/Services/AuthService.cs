using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Server.BusinessLogic.Interfaces;
using Server.Common.Models;
using Server.DataAccess.Interfaces;
using Server.DTOs.Models.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security;
using System.Security.Claims;
using System.Text;

namespace Server.BusinessLogic.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly ITokenRepository _tokenRepository;
        private readonly IAccountRepository _accountRepository;

        public AuthService(IConfiguration configuration, ITokenRepository tokenRepository, IAccountRepository accountRepository)
        {
            _configuration = configuration;
            _tokenRepository = tokenRepository;
            _accountRepository = accountRepository;
        }

        public object Generate(Account account, bool includeRefreshToken = true)
        {
            if (string.IsNullOrEmpty(account.Email))
                throw new ArgumentException("Not found email when generate token");

            // Create the claims for the JWT
            var claims = new[]
            {
                new Claim(ClaimTypes.Email, account.Email),
                new Claim(ClaimTypes.Name, account.Email),
                //new Claim(ClaimTypes.Role, "Admin")
            };

            // Get the secret key from the appsettings.json file
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtBearer:Key"]));
            // Create the signing credentials using the secret key
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Create the JWT token
            var token = new JwtSecurityToken(
                issuer: _configuration["JwtBearer:Issuer"],
                audience: _configuration["JwtBearer:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(2),
                signingCredentials: creds);

            string at = new JwtSecurityTokenHandler().WriteToken(token);

            if (includeRefreshToken)
            {
                // Create the refresh token
                var rt = Guid.NewGuid().ToString();
                return (at, rt);
            }

            return at;
        }

        public ClaimsPrincipal? Validate(string accessToken, bool validateLifetimeParam = false)
        {
            // Check if the access token is null or empty
            if (string.IsNullOrEmpty(accessToken))
                return null;

            // Create a token validation parameters object with the JWT settings from the appsettings.json file
            var tokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidIssuer = _configuration["JwtBearer:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["JwtBearer:Audience"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtBearer:Key"])),
                ValidateLifetime = validateLifetimeParam,
                ClockSkew = TimeSpan.Zero
            };

            // Create a JWT security token handler object
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();

            // Try to validate the access token and return the principal object that represents the user's identity
            try
            {
                var principal = jwtSecurityTokenHandler.ValidateToken(accessToken, tokenValidationParameters, out var securityToken);
                return principal;
            }
            catch (Exception)
            {
                // Handle the exception or return null
                return null!;
            }
        }

        public async Task<Token> GenerateNewToken(string accessToken, string refreshToken)
        {
            AccessToken? oldAccessToken = await _tokenRepository.GetAccessTokenAsync(accessToken);
            long rtIdFromCurrentAt = -1;

            if (oldAccessToken != null)
            {
                if (oldAccessToken.Revoked)
                    throw new InvalidOperationException("Access token is revoked.");

                rtIdFromCurrentAt = oldAccessToken.RefreshTokenId;

                // Validate the access token
                var principal = Validate(accessToken) ?? throw new SecurityException("Invalid access token.");

                // Get the user's email from the access token claims
                string? userEmail = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

                // Check if the user's email is null or empty
                if (string.IsNullOrEmpty(userEmail))
                    throw new ArgumentNullException(nameof(userEmail), "Invalid email.");

                // Get the account from the database by name
                Account? account = await _accountRepository.GetAccountByEmailAsync(userEmail) ?? throw new InvalidOperationException("Account not found.");

                // Get the refresh token from the database by value
                RefreshToken? oldRefreshToken = await _tokenRepository.GetRefreshTokenAsync(refreshToken) ?? throw new SecurityException("Refresh token not found.");

                // Check if the refresh token is expired
                if (oldRefreshToken.ExpirationDate < DateTime.Now)
                    throw new SecurityException("Refresh token expired.");

                // Check if the refresh token is revoked
                if (oldRefreshToken.Revoked)
                    throw new SecurityException("Refresh token revoked.");

                // Check if the refresh token belongs to the same user as the access token
                if (oldRefreshToken.AccountId != account.Id)
                    throw new SecurityException("Invalid refresh token.");

                // Check if the access token is generated by refresh token
                if (oldRefreshToken.Id != rtIdFromCurrentAt)
                    throw new SecurityException("Access token was not generated by this refresh token.");

                // Generate a new access token and a new refresh token for the user
                var token = Generate(account, false);
                if (token is string at)
                {
                    var newAccessTokenEntity = new AccessToken()
                    {
                        Value = at,
                        RefreshTokenId = oldRefreshToken.Id,
                        ExpirationDate = DateTime.Now.AddMinutes(2),
                        Revoked = false,
                    };
                    await _tokenRepository.AddAccessTokenAsync(newAccessTokenEntity);

                    await _tokenRepository.RevokeAccessToken(oldAccessToken);

                    Token newToken = new()
                    {
                        AccessToken = at,
                        RefreshToken = oldRefreshToken.Value,
                    };

                    return newToken;
                }
            }

            throw new ArgumentNullException(nameof(accessToken), "Invalid access token.");
        }

        public async Task<Account> FetchAccount(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
                throw new ArgumentException("Require access token");

            try
            {
                return await _tokenRepository.FetchAccountByAT(accessToken);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}

