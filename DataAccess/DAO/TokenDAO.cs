using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAO
{
    public class TokenDAO
    {


        public void AsignToken(string email)
        {
            using (var context = new BevososContext())
            {
                var user = context.Accounts.FirstOrDefault(a => a.Email == email);
                var token = new Models.Token();
                token.AccountId = user.UserId;
                token.TokenValue = new TokenGenerator().GenerateToken();
                context.Tokens.Add(token);
                context.SaveChanges();
            }
        }


        public bool TokenExists(string token)

        {
            using (var context = new BevososContext())
            {
                return context.Tokens.Any(t => t.TokenValue == token);
            }
        }

        public string GetToken(string email)
        {
            using (var context = new BevososContext())
            {
                return context.Tokens.FirstOrDefault(t => t.Account.Email == email).TokenValue;
            }
        }

        public bool TokenIsValid(string token, string email)
        {
            using (var context = new BevososContext())
            {
                return context.Tokens.Any(t => t.TokenValue == token && t.ExpiryDate > DateTime.Now && t.Account.Email == email);
            }
        }

        public void DeleteToken(string token, string email)
        {
            using (var context = new BevososContext())
            {
                var tokenToDelete = context.Tokens.FirstOrDefault(t => t.TokenValue == token);
                context.Tokens.Remove(tokenToDelete);
                context.SaveChanges();
            }
        }


    }
}
