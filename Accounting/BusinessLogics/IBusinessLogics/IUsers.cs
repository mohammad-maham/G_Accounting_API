using Accounting.Models;

namespace Accounting.BusinessLogics.IBusinessLogics
{
    public interface IUsers
    {
        Task<string> GetSigninAsync(string username, string password);
        Task<User?> GetSignupAsync(UserRequest userReq);
        Task<bool> IsExistUserAsync(long nationalCode);
        Task<User?> FindUserAsync(string username);
        Task<User?> FindUserAsync(string username, string password);
        Task SetPasswordAsync(string username, string password);
        Task<UserInfo> InsertUserInfoAsync(UserProfile profile);
        Task<UserInfo?> FindUserInfoAsync(long userId);
        Task<Contact> InsertUserContactsAsync(UserContact userContact);
        Task<Contact?> FindUserContactAsync(long userId);
        Task<User?> FindUserByIdAsync(long userId);
        Task UpdateUserAsync(User updatedUser);
        Task SaveUserSessionInfo(SessionInfo session);
        Task InsertUserRoleByDefaultAsync(long userId);
        Task<UserInfo> GetUserInfoByToken(string token);
    }
}
