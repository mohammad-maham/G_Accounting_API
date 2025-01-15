using Accounting.Models;

namespace Accounting.BusinessLogics.IBusinessLogics
{
    public interface IUsers
    {
        string GetSignin(string username, string? password = "");
        User? GetSignup(UserRequest userReq);
        bool IsExistUser(long nationalCode);
        User? FindUser(string username);
        User? FindUser(string username, string password);
        void SetPassword(string username, string password);
        UserInfo InsertRealUserInfo(UserProfile profile);
        LegalUserInfo InsertLegalUserInfo(LegalUserInfo profile);
        UserInfo? FindUserInfo(long userId);
        LegalUserInfo? FindLegalUserInfo(long userId);
        UserInfoVM? FindFullUserInfo(long userId);
        Contact InsertUserContacts(UserContact userContact);
        Contact? FindUserContact(long userId);
        User? FindUserById(long userId);
        void UpdateUser(User updatedUser);
        void SaveUserSessionInfo(SessionInfo session);
        void InsertUserRoleByDefault(long userId);
        UserInfo GetUserInfoByToken(string token);
        FullUserInfoVM GetFindFullUserInfo(long userId);
        List<GetUsersVM> GetUsersList();
        List<UsersList> GetUsersListByFilter(UsersList users);
        string ConvertGregDateTimeToPersianString(DateTime? date, bool onlyDate = false);
        List<Role> GetRolesList();
        List<Status> GetStatusesList();
        void ChangeUserRole(UsersRoleVM userRole);
        bool ValidateMobileNationalCode(string mobile, string nationalCode);
        bool ValidateRealUserInfo(RealUserInfoAuthVM infoAuthVM);
        LegalUserInfoAuthResult? ValidateLegalUserInfo(LegalUserInfoAuthVM infoAuthVM);
    }
}
