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
        //void AddFriend(int userId, int friendId);
        //void RemoveFriend(int userId, int friendId);
        [OperationContract]
        bool AcceptFriendRequest(int userId, int friendId, int requestId);
        [OperationContract]
        bool DeclineFriendRequest(int requestId);
        //void SendFriendRequest(int userId, int friendId);
        [OperationContract]
        bool BlockFriend(int userId, int friendId);
        [OperationContract]
        bool UnblockUser(int userId, int friendId);
        [OperationContract]
        List<FriendDTO> GetFriends(int userId);
        [OperationContract]
        bool DeleteFriend(int userId, int friendId);
        //void GetFriends(int userId);
        [OperationContract]
        List<FriendRequestDTO>GetFriendRequests(int userId);
        [OperationContract]
        List<BlockedDTO> GetBlockedUsers(int userId);
        //void GetReportedUsers(int userId);
    }

    internal interface ISocialManagerCallback
    {
        [OperationContract (IsOneWay = true)]
        void OnFriendNew(List<FriendDTO> friends);
        //void OnFriendsUpdate(int userId, OservableCollection<int> friends);
        //void OnFriendsUpdate(List<UserDto> friends);
        //void OnFriendRequestsUpdate(List<UserDto> friendRequests);
        //void OnBlockedUsersUpdate(List<UserDto> blockedUsers);
        //void OnReportedUsersUpdate(List<UserDto> reportedUsers);
    }
}
