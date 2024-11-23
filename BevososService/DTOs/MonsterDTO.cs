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
            //4	RigthArm
            //5	RightTool
            //6	Legs
            //7 Hat
        public List<CardDTO> bodyParts { get; set; }


        public MonsterDTO() { }

        public static explicit operator MonsterDTO(GameModels.Monster monster)
        {
            MonsterDTO monsterDTO = new MonsterDTO
            {
                bodyParts = new List<CardDTO>()
            };

            //nasty
            monsterDTO.bodyParts.Add(monster.Head != null ? (CardDTO)monster.Head : new CardDTO { CardId = 0 });
            monsterDTO.bodyParts.Add(monster.Torso != null ? (CardDTO)monster.Torso : new CardDTO { CardId = 0 });
            monsterDTO.bodyParts.Add(monster.LeftArm != null ? (CardDTO)monster.LeftArm : new CardDTO { CardId = 0 });
            monsterDTO.bodyParts.Add(monster.LeftHandTool != null ? (CardDTO)monster.LeftHandTool : new CardDTO { CardId = 0 });
            monsterDTO.bodyParts.Add(monster.RightArm != null ? (CardDTO)monster.RightArm : new CardDTO { CardId = 0 });
            monsterDTO.bodyParts.Add(monster.RightHandTool != null ? (CardDTO)monster.RightHandTool : new CardDTO { CardId = 0 });
            monsterDTO.bodyParts.Add(monster.Legs != null ? (CardDTO)monster.Legs : new CardDTO { CardId = 0 });
            monsterDTO.bodyParts.Add(monster.Hat != null ? (CardDTO)monster.Hat : new CardDTO { CardId = 0 });

            return monsterDTO;
        }


    }
}
