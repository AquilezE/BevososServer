using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace BevososService.DTOs
{

    [DataContract]
    public class PlayerStateDTO
    {

        [DataMember]
        public UserDTO User { get; set; }

        [DataMember]
        public List<CardDTO> Hand { get; set; } = new List<CardDTO>();

        [DataMember]
        public List<MonsterDTO> Monsters { get; set; } = new List<MonsterDTO>();

        public bool Disconnected { get; set; } = false;

        public static explicit operator PlayerStateDTO(GameModels.PlayerState playerState)
        {
            return new PlayerStateDTO
            {
                User = playerState.User,
                Hand = playerState.Hand.Select(card => (CardDTO)card).ToList(),
                Monsters = playerState.Monsters.Select(monster => (MonsterDTO)monster).ToList(),
                Disconnected = playerState.Disconnected
            };
        }

    }

}