using Server.Common.Models;

namespace Server.DataAccess.Interfaces
{
    public interface IAccountRepository
    {
        bool CheckAccountExist(string email);
        Task AddAccountAsync(Account account);
        Task<Account?> GetAccountByEmailAsync(string email);
        Task<Account> GetAccountByAccountIdAsync(long accountId);
    }
}