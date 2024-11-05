using BevososService.DTOs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace BevososService
{
    [ServiceContract(CallbackContract = typeof(ISocialManagerCallback))]
    internal interface ISocialManager
    {

        [OperationContract]
        void Connect(int userId);

        [OperationContract]
        void Disconnect(int userId);


        [OperationContract]
        bool SendFriendRequest(int userId, int requesteeId);

        [OperationContract]
        void AcceptFriendRequest(int userId, int friendId, int requestId);
        [OperationContract]
        bool DeclineFriendRequest(int requestId);
        [OperationContract]
        bool BlockFriend(int userId, int friendId);
        [OperationContract]
        bool UnblockUser(int userId, int friendId);

        [OperationContract]
        bool BlockUser(int userId, int friendId);

        [OperationContract]
        bool DeleteFriend(int userId, int friendId);
        [OperationContract]
        List<FriendDTO> GetFriends(int userId);

        [OperationContract]
        List<FriendRequestDTO> GetFriendRequests(int userId);

        [OperationContract]
        List<BlockedDTO> GetBlockedUsers(int userId);

        [OperationContract]
        List<UserDto> GetUsersFoundByName(int userId, string name);
    }

    internal interface ISocialManagerCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnFriendOnline(int friendId);

        [OperationContract(IsOneWay = true)]
        void OnFriendOffline(int friendId);

        [OperationContract(IsOneWay = true)]
        void OnNewFriend(FriendDTO friend);

        [OperationContract(IsOneWay = true)]
        void OnNewFriendRequest(FriendRequestDTO friendRequest);

        [OperationContract(IsOneWay = true)]
        void OnFriendshipDeleted(int friendId);

    }
}
