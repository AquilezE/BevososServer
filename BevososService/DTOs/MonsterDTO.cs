using System.Collections.Generic;

namespace BevososService.DTOs
{
    public class MonsterDTO
    {

        public List<CardDTO> BodyParts { get; set; } = new List<CardDTO>();


        public static explicit operator MonsterDTO(GameModels.Monster monster)
        {
            var monsterDTO = new MonsterDTO();

            monsterDTO.BodyParts.Add(monster.Head != null ? (CardDTO)monster.Head : new CardDTO { CardId = 0 });
            monsterDTO.BodyParts.Add(monster.Torso != null ? (CardDTO)monster.Torso : new CardDTO { CardId = 0 });
            monsterDTO.BodyParts.Add(monster.LeftArm != null ? (CardDTO)monster.LeftArm : new CardDTO { CardId = 0 });
            monsterDTO.BodyParts.Add(monster.LeftHandTool != null
                ? (CardDTO)monster.LeftHandTool
                : new CardDTO { CardId = 0 });
            monsterDTO.BodyParts.Add(monster.RightArm != null ? (CardDTO)monster.RightArm : new CardDTO { CardId = 0 });
            monsterDTO.BodyParts.Add(monster.RightHandTool != null
                ? (CardDTO)monster.RightHandTool
                : new CardDTO { CardId = 0 });
            monsterDTO.BodyParts.Add(monster.Legs != null ? (CardDTO)monster.Legs : new CardDTO { CardId = 0 });
            monsterDTO.BodyParts.Add(monster.Hat != null ? (CardDTO)monster.Hat : new CardDTO { CardId = 0 });

            return monsterDTO;
        }
    }
}