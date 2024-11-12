using BevososService.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace BevososService
{
    [ServiceContract (CallbackContract = typeof(IGameManagerCallback))]
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
        void JoinGame(int gameId,UserDto userDto);

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

    }

}
