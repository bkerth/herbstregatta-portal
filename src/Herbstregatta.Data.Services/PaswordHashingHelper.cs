using System.Security.Cryptography;
using System.Text;

namespace Herbstregatta.Data.Services
{
    public record HashedPasswordData
    {
        /// <summary>
        /// The hashed passord
        /// </summary>
        public string Hash { get; init; } = default!;

        /// <summary>
        /// The salt that was used to hash the password
        /// </summary>
        public string Salt { get; init; } = default!;
    }

    public static class PaswordHashingHelper
    {
        public static HashedPasswordData CreatePasswordHash(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException("password");
            }
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            byte[] password_buffer = Encoding.UTF8.GetBytes(password);
            byte[] passwordWithSalt = new byte[salt.Length + password.Length];
            Buffer.BlockCopy(salt, 0, passwordWithSalt, 0, salt.Length);
            Buffer.BlockCopy(password_buffer, 0, passwordWithSalt, salt.Length, password_buffer.Length);

            string saltString = Convert.ToHexString(salt);

            string? hashedPassword = string.Empty;

            // Uses SHA256 to create the hash            
            using (var alg = SHA512.Create())
            {

                var hashValue = alg.ComputeHash(passwordWithSalt);
                hashedPassword = Convert.ToHexString(hashValue);
            }

            if (string.IsNullOrEmpty(hashedPassword))
            {
                throw new InvalidOperationException("Could not create a hashed password!");
            }

            return new HashedPasswordData { Hash = hashedPassword, Salt = saltString };
        }

        public static string HashPassword(string plainPassword, string salt)
        {
            byte[] salt_buffer = Convert.FromHexString(salt);
            byte[] password_buffer = Encoding.UTF8.GetBytes(plainPassword);
            byte[] passwordWithSalt = new byte[salt_buffer.Length + password_buffer.Length];
            Buffer.BlockCopy(salt_buffer, 0, passwordWithSalt, 0, salt_buffer.Length);
            Buffer.BlockCopy(password_buffer, 0, passwordWithSalt, salt_buffer.Length, password_buffer.Length);

            string? hashedPassword = string.Empty;

            // Uses SHA256 to create the hash            
            using (var alg = SHA512.Create())
            {

                var hashValue = alg.ComputeHash(passwordWithSalt);
                hashedPassword = Convert.ToHexString(hashValue);
            }
            if (string.IsNullOrEmpty(hashedPassword))
            {
                throw new InvalidOperationException("Could not create a hashed password!");
            }
            return hashedPassword;
        }
    }
}
