using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
