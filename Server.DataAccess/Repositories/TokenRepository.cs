using Microsoft.EntityFrameworkCore;
using Server.Common.Models;
using Server.DataAccess.Interfaces;

namespace Server.DataAccess.Repositories
{
    public class TokenRepository : ITokenRepository
    {
        private readonly AppDbContext _db;

        public TokenRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddRefreshTokenAsync(RefreshToken refreshToken)
        {
            if (_db.RefreshTokens == null)
                throw new Exception();

            _db.RefreshTokens.Add(refreshToken);
            await _db.SaveChangesAsync();
        }

        public async Task<RefreshToken> GetRefreshTokenAsync(string? refreshTokenValue)
        {
            if (_db.RefreshTokens == null)
                throw new Exception();

            return await _db.RefreshTokens.Where(r => r.Value == refreshTokenValue).FirstAsync() ?? throw new Exception("Refresh Token not found");
        }

        public async Task<AccessToken> GetAccessTokenAsync(string accessTokenValue)
        {
            if (_db.AccessTokens == null)
                throw new Exception();

            return await _db.AccessTokens.Where(a => a.Value == accessTokenValue).FirstAsync() ?? throw new Exception("Access Token not found");
        }

        public async Task AddAccessTokenAsync(AccessToken accessToken)
        {
            if (_db.AccessTokens == null)
                throw new Exception();

            _db.AccessTokens.Add(accessToken);
            await _db.SaveChangesAsync();
        }

        public async Task RevokeAccessToken(AccessToken token)
        {
            token.Revoked = true;
            await _db.SaveChangesAsync();
        }

        public async Task<Account> FetchAccountByAT(string accessToken)
        {
            if (_db.AccessTokens != null && _db.RefreshTokens != null && _db.Accounts != null)
            {
                return await (from at in _db.AccessTokens
                              join rt in _db.RefreshTokens on at.RefreshTokenId equals rt.Id
                              join a in _db.Accounts on rt.AccountId equals a.Id
                              where at.Value == accessToken
                              select a).FirstAsync() ?? throw new Exception("Account not found from this access token");
            }

            throw new Exception();
        }
    }
}