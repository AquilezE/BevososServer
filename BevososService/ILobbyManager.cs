using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace BevososService
{

    [ServiceContract(CallbackContract = typeof(ILobbyManagerCallback))]

    public interface ILobbyManager
    {
        [OperationContract(IsOneWay = true)]
        void NewLobbyCreated(int UserId);

        [OperationContract(IsOneWay = true)]
        void JoinLobby(int lobbyId, UserDto userDto);

        [OperationContract(IsOneWay = true)]
        void SendMessage(int lobbyId, int UserId, string message);

        [OperationContract(IsOneWay = true)]
        void LeaveLobby(int lobbyId, int UserId);

        [OperationContract(IsOneWay = true)]
        void KickUser(int lobbyId, int UserId);
    }

    [ServiceContract]
    internal interface ILobbyManagerCallback
    {
        [OperationContract]
        void OnNewLobbyCreated(int lobbyId, int UserId);

        [OperationContract]
        void OnJoinLobby(int lobbyId, UserDto userDto);

        [OperationContract]
        void OnLeaveLobby(int lobbyId, int UserId);

        [OperationContract]
        void OnSendMessage(int UserId, string message);

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
