using System.Net.Mail;

namespace Accounting.Helpers
{
    public class EmailValidator
    {
        public static bool IsValidEmail(string email)
        {
            string? trimmedEmail = email.Trim();

            if (trimmedEmail.EndsWith("."))
            {
                return false;
            }

            if (trimmedEmail.All(char.IsDigit))
            {
                return false;
            }

            if (!trimmedEmail.EndsWith(".com"))
            {
                return false;
            }

            try
            {
                MailAddress? addr = new(email);
                return addr.Address == trimmedEmail;
            }
            catch
            {
                return false;
            }
        }
    }
}
