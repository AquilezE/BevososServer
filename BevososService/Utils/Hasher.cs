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
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                    byte[] hashBytes = sha256.ComputeHash(passwordBytes);
                    string hashedPassword = Convert.ToBase64String(hashBytes);
                    return hashedPassword;
                }
            }

            public static bool VerifyPassword(string password, string hashedPassword)
            {
                string hashedInput = HashPassword(password);
                return hashedInput == hashedPassword;
            }
        }
    }
}
