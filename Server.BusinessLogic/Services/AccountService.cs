using Server.BusinessLogic.Interfaces;
using Server.Common.Models;
using Server.DataAccess.Interfaces;
using Server.DTOs.Models.Auth;
using Server.DTOs.Models.Login;
using Server.DTOs.Models.Register;

namespace Server.BusinessLogic.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAuthService _authService;
        private readonly IAccountRepository _accountRepository;
        private readonly ITokenRepository _tokenRepository;

        public AccountService(IAuthService authService, IAccountRepository accountRepository, ITokenRepository tokenRepository)
        {
            _authService = authService;
            _accountRepository = accountRepository;
            _tokenRepository = tokenRepository;
        }

        public async Task<Token> RegisterAsync(RegisterRequest request)
        {
            if (string.IsNullOrEmpty(request.Email))
                throw new ArgumentException("Email is required");

            if (_accountRepository.CheckAccountExist(request.Email))
                throw new InvalidOperationException("This email already exists!");

            string passwordHashed = BCrypt.Net.BCrypt.HashPassword(request.Password);

            Account account = new()
            {
                Email = request.Email,
                Password = passwordHashed,
            };

            await _accountRepository.AddAccountAsync(account);

            var token = _authService.Generate(account);
            if (token is (string at, string rt))
            {
                RefreshToken refreshToken = new()
                {
                    Value = rt,
                    AccountId = account.Id,
                    ExpirationDate = DateTime.Now.AddDays(7),
                    Revoked = false,
                };
                await _tokenRepository.AddRefreshTokenAsync(refreshToken);

                var accessToken = new AccessToken()
                {
                    Value = at,
                    RefreshTokenId = refreshToken.Id,
                    ExpirationDate = DateTime.Now.AddDays(2),
                    Revoked = false,
                };
                await _tokenRepository.AddAccessTokenAsync(accessToken);

                Token returnToken = new()
                {
                    AccessToken = at,
                    RefreshToken = rt,
                };

                return returnToken;
            }

            throw new InvalidOperationException("Failed to generate tokens.");
        }

        public async Task<Token> LoginAsync(LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email))
                throw new ArgumentException("Email is required");

            Account? account = await _accountRepository.GetAccountByEmailAsync(request.Email) ?? throw new Exception("Account not found");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, account.Password))
                throw new Exception("Incorrect Password");

            var token = _authService.Generate(account);
            if (token is (string at, string rt))
            {
                RefreshToken refreshToken = new()
                {
                    Value = rt,
                    AccountId = account.Id,
                    ExpirationDate = DateTime.Now.AddDays(7),
                    Revoked = false,
                };
                await _tokenRepository.AddRefreshTokenAsync(refreshToken);

                AccessToken accessToken = new()
                {
                    Value = at,
                    RefreshTokenId = refreshToken.Id,
                    ExpirationDate = DateTime.Now.AddDays(2),
                    Revoked = false,
                };
                await _tokenRepository.AddAccessTokenAsync(accessToken);

                Token returnToken = new()
                {
                    AccessToken = at,
                    RefreshToken = rt,
                };

                return returnToken;
            }

            throw new InvalidOperationException("Failed to generate tokens.");
        }
    }
}