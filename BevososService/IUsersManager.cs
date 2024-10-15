using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

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
        void SendToken(string email);

        [OperationContract]
        bool VerifyToken(string email, string token);


        [OperationContract]

        bool RegisterUser(string email, string username, string password);

        [OperationContract]

        UserDto LogIn(string email, string password);

    }


    [DataContract]
    public class UserDto
    {
        [DataMember]
        public int UserId { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public int ProfilePictureId { get; set; }
    }



}
