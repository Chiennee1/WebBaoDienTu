using System;
using System.Security.Cryptography;
using System.Text;

namespace baoDienTu.Helpers
{
    public static class SecurityHelper
    {
        public static string GenerateSalt()
        {
            var bytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes);
        }

        public static string GenerateToken()
        {
            return Guid.NewGuid().ToString("N");
        }

        public static string HashPassword(string password, string salt)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes((salt ?? string.Empty) + (password ?? string.Empty)));
                var builder = new StringBuilder(bytes.Length * 2);
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
