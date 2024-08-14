using Server.DTOs.Models.Auth;
using Server.DTOs.Models.Login;
using Server.DTOs.Models.Register;

namespace Server.BusinessLogic.Interfaces
{
    public interface IAccountService
    {
        Task<Token> RegisterAsync(RegisterRequest request);
        Task<Token> LoginAsync(LoginRequest request);
    }
}