using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BevososService.DTOs;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace BevososService
{

    [ServiceContract(CallbackContract = typeof(ILobbyManagerCallback))]

    public interface ILobbyManager
    {
        [OperationContract(IsOneWay = true)]
        void NewLobbyCreated(UserDto userDto);

        [OperationContract(IsOneWay = true)]
        void JoinLobby(int lobbyId, UserDto userDto);

        [OperationContract(IsOneWay = true)]
        void SendMessage(int lobbyId, int UserId, string message);

        [OperationContract(IsOneWay = true)]
        void LeaveLobby(int lobbyId, int UserId);

        [OperationContract(IsOneWay = true)]
        void KickUser(int lobbyId, int kickerId, int targetUserId, string reason);
    }

    [ServiceContract]
    internal interface ILobbyManagerCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnNewLobbyCreated(int lobbyId, int UserId);

        [OperationContract(IsOneWay = true)]
        void OnJoinLobby(int lobbyId, UserDto userDto);

        [OperationContract(IsOneWay = true)]
        void OnLeaveLobby(int lobbyId, int UserId);

        [OperationContract(IsOneWay = true)]
        void OnSendMessage(int UserId, string message);
        
        [OperationContract(IsOneWay = true)]
        void OnLobbyUsersUpdate(int lobbyId, List<UserDto> users);

        [OperationContract(IsOneWay = true)]
        void OnLeaderChanged(int lobbyId, int newLeaderId);

        [OperationContract(IsOneWay = true)]
        void OnKicked(int lobbyId, string reason);

    }

    [ServiceContract]
    public interface ILobbyChecker
    {
        [OperationContract]
        bool IsLobbyOpen(int lobbyId);

        [OperationContract]
        bool IsLobbyFull(int lobbyId);
    }
}
