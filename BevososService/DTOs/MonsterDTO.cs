using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BevososService.DTOs
{
    public class MonsterDTO
    {

            //this is going to be the CardDTO of the cards that represent the body parts of the monster
            //0	Head       
            //1	Torso      
            //2	LeftArm
            //3	LeftTool
            //4	RightArm
            //5	RightTool
            //6	Legs
            //7 Hat
        public List<CardDTO> BodyParts { get; set; } = new List<CardDTO>();


        public MonsterDTO() { }

        public static explicit operator MonsterDTO(GameModels.Monster monster)
        {
            MonsterDTO monsterDTO = new MonsterDTO();

            //nasty
            monsterDTO.BodyParts.Add(monster.Head != null ? (CardDTO)monster.Head : new CardDTO { CardId = 0 });
            monsterDTO.BodyParts.Add(monster.Torso != null ? (CardDTO)monster.Torso : new CardDTO { CardId = 0 });
            monsterDTO.BodyParts.Add(monster.LeftArm != null ? (CardDTO)monster.LeftArm : new CardDTO { CardId = 0 });
            monsterDTO.BodyParts.Add(monster.LeftHandTool != null ? (CardDTO)monster.LeftHandTool : new CardDTO { CardId = 0 });
            monsterDTO.BodyParts.Add(monster.RightArm != null ? (CardDTO)monster.RightArm : new CardDTO { CardId = 0 });
            monsterDTO.BodyParts.Add(monster.RightHandTool != null ? (CardDTO)monster.RightHandTool : new CardDTO { CardId = 0 });
            monsterDTO.BodyParts.Add(monster.Legs != null ? (CardDTO)monster.Legs : new CardDTO { CardId = 0 });
            monsterDTO.BodyParts.Add(monster.Hat != null ? (CardDTO)monster.Hat : new CardDTO { CardId = 0 });

            return monsterDTO;
        }


    }
}
