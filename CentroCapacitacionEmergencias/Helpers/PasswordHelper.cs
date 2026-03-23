using System.Security.Cryptography;
using System.Text;

namespace CentroCapacitacionEmergencias.Helpers
{
    public static class PasswordHelper
    {
        public static string Hash(string input)
        {
            input = (input ?? "").Trim();

            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder sb = new StringBuilder();

                foreach (byte b in bytes)
                {
                    sb.Append(b.ToString("X2"));
                }

                return sb.ToString();
            }
        }
    }
}