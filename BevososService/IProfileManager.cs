
using System.ServiceModel;


namespace BevososService
{
    [ServiceContract (CallbackContract = typeof ( IProfileManagerCallback))]

    internal interface IProfileManager
    {
        [OperationContract(IsOneWay = true)]
        void UpdateProfile(int userId, string username, int profilePictureId);

        [OperationContract(IsOneWay = true)]
        void ChangePassword(int userId, string oldPassword, string newPassword);
    }

    [ServiceContract]
    internal interface IProfileManagerCallback
    {
        [OperationContract]
        void OnProfileUpdate(string username, int profilePictureId, string error);

        [OperationContract]
        void OnPasswordChange(string error);
    }
}