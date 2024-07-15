namespace Accounting.Helpers
{
    public class NationalCodeValidator
    {
        public static bool IsValidNationalCode(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return false;
            }

            if (string.IsNullOrEmpty(username))
            {
                return false;
            }

            return username.All(char.IsDigit) || username.Length == 10;
        }
    }
}
