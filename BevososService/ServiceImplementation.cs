using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using BevososService.Utils;
using DataAccess.DAO;

namespace BevososService
{
    public partial class ServiceImplementation : IUsersManager
    {
        public bool IsEmailTaken(string email)
        {
             return new AccountDAO().EmailExists(email);
        }

        public bool IsUsernameTaken(string username)
        {
            return new UserDAO().UsernameExists(username);
        }

        public bool RegisterUser(UserDto user)
        {
            throw new NotImplementedException();
        }

        public void SendToken(string email)
        {
            if(new TokenDAO().TokenExists(email))
            {
               EmailUtils.SendTokenByEmail(email, new TokenDAO().GetToken(email));
            }else
            {
                new TokenDAO().AsignToken(email);
                EmailUtils.SendTokenByEmail(email, new TokenDAO().GetToken(email));
            }
        }

        public bool VerifyToken(string email, string token)
        {
            if (new TokenDAO().TokenExists(token))
            {
                if(new TokenDAO().TokenIsValid(token, email))
                {
                    new TokenDAO().DeleteToken(token, email);
                    return true;
                }
            }
            return false;
        }


        
        public UserDto LogIn(string email, string password)
        {
            throw new NotImplementedException();
        }

    }
}
