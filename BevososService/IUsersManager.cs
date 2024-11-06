using System.ServiceModel;
using BevososService.DTOs;

namespace BevososService
{
    [ServiceContract]
    internal interface IUsersManager
    {

        [OperationContract]

        bool IsUsernameTaken(string username);

        [OperationContract]
        bool IsEmailTaken(string email);


        [OperationContract]
        bool SendToken(string email);

        [OperationContract]
        bool VerifyToken(string email, string token);


        [OperationContract]

        bool RegisterUser(string email, string username, string password);

        [OperationContract]

        UserDto LogIn(string email, string password);

        [OperationContract]
        bool RecoverPassword(string email, string newPasswordHash);

        
    }


}
