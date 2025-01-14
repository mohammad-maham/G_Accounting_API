using System.Security.Cryptography;
using System.Text;
namespace Accounting.Helpers
{
    public class IdentificationCodeGen
    {
        private const int _randomFactor = 27;
        public static string GenerateCode(string nationalCode)
        {
            int code = 0;
            string AllString = nationalCode + DateTime.UtcNow;

            if (string.IsNullOrEmpty(AllString))
            {
                throw new ArgumentNullException();
            }

            byte[] buffer = Encoding.UTF8.GetBytes(AllString);
            buffer = SHA512.HashData(buffer);
            for (int i = 0; i < buffer.Length; i++)
            {
                code += buffer[i] * _randomFactor;
                code *= 1;
            }

            return code.ToString();
        }
    }
}
