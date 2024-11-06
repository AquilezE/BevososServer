using System;
using System.Linq;


namespace DataAccess.DAO
{
    public class TokenDAO
    {


        public int AsignToken(string email)
        {
            using (var context = new BevososContext())
            {
                var existingToken = context.Tokens.FirstOrDefault(t => t.Email == email);
                if (existingToken != null)
                {
                    existingToken.TokenValue = new TokenGenerator().GenerateToken();
                    existingToken.ExpiryDate = DateTime.Now.AddMinutes(15);
                }
                else
                {
                    var token = new Models.Token();
                    token.Email = email;
                    token.TokenValue = new TokenGenerator().GenerateToken();
                    token.ExpiryDate = DateTime.Now.AddMinutes(15);
                    context.Tokens.Add(token);
                }

                int affectedRows = context.SaveChanges();
                return affectedRows;
            }
        }

        public bool HasToken(string email)
        {
            using (var context = new BevososContext())
            {
                return context.Tokens.Any(t => t.Email == email && t.ExpiryDate>DateTime.Now);
            }
        }

        public string GetToken(string email)
        {
            using (var context = new BevososContext())
            {
                var token = context.Tokens.FirstOrDefault(t => t.Email == email);

                if (token == null)
                {
                    return "-1";
                }

                return token.TokenValue;
            }
        }

        public bool TokenIsValid(string token, string email)
        {
            using (var context = new BevososContext())
            {
                return context.Tokens.Any(t => t.TokenValue == token && t.ExpiryDate > DateTime.Now && t.Email == email);
            }
        }

        public bool DeleteToken(string token, string email)
        {
            using (var context = new BevososContext())
            {
                var tokenToDelete = context.Tokens.FirstOrDefault(t => t.TokenValue == token && t.Email == email);

                if (tokenToDelete != null)
                {
                    context.Tokens.Remove(tokenToDelete);
                    int affectedRows = context.SaveChanges();
                    return affectedRows > 0;
                }
                return false;
            }
        }



    }
}
