using BevososService.DTOs;
using BevososService.GameModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace BevososService
{
    [ServiceContract(CallbackContract = typeof(IGameManagerCallback))]
    public interface IGameManager
    {
        /// <summary>
        /// Allows a player to join an active game session. This method registers the player's callback
        /// and sends the current game state to the joining player.
        /// </summary>
        /// <param name="gameId">The unique identifier of the game session.</param>
        /// <param name="userDto">The data transfer object containing information about the user joining the game.</param>
        /// <exception cref="FaultException">Thrown when the game does not exist or the player is not found in the game.</exception>
        [OperationContract(IsOneWay = true)]
        void JoinGame(int gameId, UserDto userDto);

        /// <summary>
        /// Handles the logic for a player drawing a card during their turn. Updates the player's hand
        /// and sends the updated game state to all players in the match.
        /// </summary>
        /// <param name="matchCode">The unique code representing the match.</param>
        /// <param name="userId">The unique identifier of the user drawing the card.</param>
        [OperationContract(IsOneWay = true)]
        void DrawCard(int matchCode, int userId);


        /// <summary>
        /// Handles the logic for a player playing a card during their turn. Updates the game state
        /// and sends the updated game state to all players in the match.
        /// </summary>
        /// <param name="userId">The unique id of the player that played the card.</param>
        /// <param name="matchCode">The unique code representing the match.</param>
        /// <param name="cardId">The unique identifier of the card being played.</param>

        [OperationContract(IsOneWay = true)]
        void PlayCard(int userId, int matchCode, int cardId);

        /// <summary>
        /// Executes the action of provoking a specific baby pile during a player's turn. This method handles the logic for a player provoking a baby pile,
        /// updates the game state, and sends the updated game state to all players in the match.
        /// </summary>
        /// <param name="userId">The unique identifier of the player provoking the baby pile.</param>
        /// <param name="matchCode">The unique code representing the match.</param>
        /// <param name="babyPileProvoked">The index of the baby pile being provoked.</param>

        [OperationContract(IsOneWay = true)]
        void PlayProvoke(int userId, int matchCode, int babyPileProvoked);


        /// <summary>
        /// Executes the action of placing a body part card on a monster during a player's turn. This method handles the logic for a player placing a body part card,
        /// updates the game state, and sends the updated game state to all players in the match.
        /// </summary>
        /// <param name="userId">The unique identifier of the player placing the body part card.</param>
        /// <param name="matchCode">The unique code representing the match.</param>
        /// <param name="cardId">The unique identifier of the body part card being placed.</param>
        /// <param name="monsterSelectedIndex">The index of the monster on which the body part card is being placed.</param>
        [OperationContract(IsOneWay = true)]
        void ExecuteBodyPartPlacement(int userId, int matchCode, int cardId, int monsterSelectedIndex);

        /// <summary>
        /// Executes the action of placing a tool card on a monster during a player's turn. This method handles the logic for a player placing a tool card,
        /// updates the game state, and sends the updated game state to all players in the match.
        /// </summary>
        /// <param name="userId">The unique identifier of the player placing the tool card.</param>
        /// <param name="matchCode">The unique code representing the match.</param>
        /// <param name="cardId">The unique identifier of the tool card being placed.</param>
        /// <param name="monsterSelectedIndex">The index of the monster on which the tool card is being placed.</param>
        [OperationContract(IsOneWay = true)]
        void ExecuteToolPlacement(int userId, int matchCode, int cardId, int monsterSelectedIndex);

        /// <summary>
        /// Executes the action of placing a hat card on a monster during a player's turn. This method handles the logic for a player placing a hat card,
        /// updates the game state, and sends the updated game state to all players in the match.
        /// </summary>
        /// <param name="userId">The unique identifier of the player placing the hat card.</param>
        /// <param name="matchCode">The unique code representing the match.</param>
        /// <param name="cardId">The unique identifier of the hat card being placed.</param>
        /// <param name="monsterSelectedIndex">The index of the monster on which the hat card is being placed.</param>
        [OperationContract(IsOneWay = true)]
        void ExecuteHatPlacement(int userId, int matchCode, int cardId, int monsterSelectedIndex);

        /// <summary>
        /// Executes the action of provoking a specific baby pile during a player's turn. This method handles the logic for a player provoking a baby pile,
        /// updates the game state, and sends the updated game state to all players in the match.
        /// </summary>
        /// <param name="userId">The unique identifier of the player provoking the baby pile.</param>
        /// <param name="matchCode">The unique code representing the match.</param>
        /// <param name="babyPileProvoked">The index of the baby pile being provoked.</param>
        [OperationContract(IsOneWay = true)]
        void ExecuteProvoke(int userId, int matchCode, int babyPileProvoked);

    }

    [ServiceContract]
    public interface IGameManagerCallback
    {
        /// <summary>
        /// Sends the current game state to a player. This is used to update the player's view
        /// with the latest game status, including player hands, card decks, and game progress.
        /// </summary>
        /// <param name="gameStateDto">A data transfer object representing the current state of the game.</param>
        [OperationContract(IsOneWay = true)]
        void ReceiveGameState(GameStateDTO gameStateDto);

        /// <summary>
        /// Requests the selection of a body part for customization.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="matchCode">The unique code representing the match.</param>
        /// <param name="card">The card that was played that triggered the request.</param>
        [OperationContract(IsOneWay = true)]
        void RequestBodyPartSelection(int userId, int matchCode, CardDTO card);

        /// <summary>
        /// Requests the selection of a tool for customization.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="matchCode">The unique code representing the match.</param>
        /// <param name="card">The card that was played that triggered the request.</param>
        [OperationContract(IsOneWay = true)]
        void RequestToolSelection(int userId, int matchCode, CardDTO card);

        /// <summary>
        /// Requests the selection of a hat for customization.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="matchCode">The unique code representing the match.</param>
        /// <param name="card">The card that was played that triggered the request.</param>
        [OperationContract(IsOneWay = true)]
        void RequestHatSelection(int userId, int matchCode, CardDTO card);



        /// <summary>
        /// Requests the selection of a baby pile action during the game.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="matchCode">The unique code representing the match.</param>
        [OperationContract(IsOneWay = true)]
        void RequestProvokeSelection(int userId, int matchCode);



        /// <summary>
        /// Notifies the player that an action they attempted to perform is invalid.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="messageKey">The key representing the message describing the invalid action.</param>
        [OperationContract(IsOneWay = true)]
        void NotifyActionInvalid(string messageKey);



        /// <summary>
        /// Notifies the player that a specific baby pile has been provoked during the game.
        /// </summary>
        /// <param name="matchCode">The unique code representing the match.</param>
        /// <param name="babyPileIndex">The index of the provoked baby pile.</param>
        [OperationContract(IsOneWay = true)]
        void OnProvoke(int matchCode, int babyPileIndex);

        /// <summary>
        /// Notifies the player that the end game phase has started.
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void OnNotifyEndGamePhase();

        /// <summary>
        /// Notifies the player that the game has ended.
        /// </summary>
        /// <param name="matchCode">The unique code representing the match.</param>
        [OperationContract(IsOneWay = true)]
        void OnNotifyGameEnded(int matchCode, List<StatsDTO> stats);

        /// <summary>
        /// Notifies the player that the game has ended without any users.
        /// </summary>
        /// <param name="matchCode">The unique code representing the match.</param>
        [OperationContract(IsOneWay = true)]
        void OnNotifyGameEndedWithoutUsers(int matchCode);
    }

}
