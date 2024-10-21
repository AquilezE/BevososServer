using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace BevososService
{
    [ServiceContract (CallbackContract = typeof (IProfileManagerCallback))]

    internal interface IProfileManager
    {
        [OperationContract(IsOneWay = true)]
        void UpdateProfile(int userId, string username, int profilePictureId);

    }

    [ServiceContract]
    internal interface IProfileManagerCallback
    {
        [OperationContract]
        void OnProfileUpdate(string username, int profilePictureId, string error);
    }
}