using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace BevososService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "ServiceImplementation" in both code and config file together.
    public class ServiceImplementation : IUsersManager
    {
        public bool IsEmailTaken(string email)
        {
            throw new NotImplementedException();
        }

        public bool IsUsernameTaken(string username)
        {
            throw new NotImplementedException();
        }

        public bool RegisterUser(string email, string username, string password)
        {
            throw new NotImplementedException();
        }

        public bool SendToken(string email)
        {
            throw new NotImplementedException();
        }

        public bool VerifyToken(string email, int token)
        {
            throw new NotImplementedException();
        }
    }
}
