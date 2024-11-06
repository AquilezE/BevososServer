using System;

namespace DataAccess
{
    public class TokenGenerator
    {
        public string GenerateToken()
        {
            Random random = new Random();
            int tokenNumber = random.Next(0, 1000000);
            string token = tokenNumber.ToString("D6");
            return token;
        }
    }
}
