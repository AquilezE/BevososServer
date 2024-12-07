using System;

using System.Security.Cryptography;
using System.Text;


namespace BevososService.Utils
{
    public class Hasher
    {


        //Aqui podriamos usar BCrypt, pero por simplicidad usaremos SHA256
        public class SimpleHashing
        {
            protected SimpleHashing() { }

            public static string HashPassword(string password)
            {
                if (password == null)
                    throw new ArgumentNullException(nameof(password));

                using (var sha256 = SHA256.Create())
                {
                    byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                    byte[] hashBytes = sha256.ComputeHash(passwordBytes);
                    return Convert.ToBase64String(hashBytes);
                }
            }

            public static bool VerifyPassword(string password, string hashedPassword)
            {
                if (password == null)
                    throw new ArgumentNullException(nameof(password));
                if (hashedPassword == null)
                    throw new ArgumentNullException(nameof(hashedPassword));

                string hashedInput = HashPassword(password);
                return hashedInput == hashedPassword;
            }
        }
    }
}
