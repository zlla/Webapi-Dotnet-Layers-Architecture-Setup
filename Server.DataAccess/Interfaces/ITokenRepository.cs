using Server.Common.Models;

namespace Server.DataAccess.Interfaces
{
    public interface ITokenRepository
    {
        Task AddRefreshTokenAsync(RefreshToken refreshToken);
        Task AddAccessTokenAsync(AccessToken accessToken);
        Task<RefreshToken> GetRefreshTokenAsync(string refreshTokenValue);
        Task<AccessToken> GetAccessTokenAsync(string accessTokenValue);
        Task RevokeAccessToken(AccessToken token);
        Task<Account> FetchAccountByAT(string accessToken);
    }
}