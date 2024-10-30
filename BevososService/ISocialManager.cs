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
        //void AcceptFriendRequest(int userId, int friendId);
        //void DeclineFriendRequest(int userId, int friendId);
        //void SendFriendRequest(int userId, int friendId);
        //void BlockUser(int userId, int friendId);
        //void UnblockUser(int userId, int friendId);
        //void ReportUser(int userId, int friendId);
        [OperationContract]
        List<FriendDTO> GetFriends(int userId);
        //void GetFriends(int userId);
        [OperationContract]
        List<FriendRequestDTO>GetFriendRequests(int userId);
        //void GetBlockedUsers(int userId);
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
