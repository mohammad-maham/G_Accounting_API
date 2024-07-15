using System.Security.Cryptography;
using System.Text;

namespace Accounting.Helpers
{
    public static class SecurePasswordHasher
    {
        public static string Hash(string password)
        {
            using SHA256 sha256 = SHA256.Create();
            // Send a sample text to hash.  
            byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            // Get the hashed string.  
            string hash = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            // Print the string.   
            return hash;
        }
    }
}
