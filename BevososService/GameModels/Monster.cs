using System;
using System.Collections.Generic;

namespace BevososService.GameModels
{

    public class Monster
    {

        public Card Head { get; set; }
        public Card Torso { get; set; }
        public Card LeftArm { get; set; }
        public Card LeftHandTool { get; set; }
        public Card RightArm { get; set; }
        public Card RightHandTool { get; set; }
        public Card Legs { get; set; }
        public Card Hat { get; set; }

        private static readonly Dictionary<int, Func<Monster, Card, bool>> PartSetters
            = new Dictionary<int, Func<Monster, Card, bool>>
            {
                { Card.HeadIndex, (monster, card) => monster.TrySetHead(card) },
                { Card.BodyIndex, (monster, card) => monster.TrySetTorso(card) },
                { Card.LeftArmIndex, (monster, card) => monster.TrySetLeftArm(card) },
                { Card.LeftArmToolIndex, (monster, card) => monster.TrySetLeftHandTool(card) },
                { Card.RightArmIndex, (monster, card) => monster.TrySetRightArm(card) },
                { Card.RightArmToolIndex, (monster, card) => monster.TrySetRightHandTool(card) },
                { Card.LegsIndex, (monster, card) => monster.TrySetLegs(card) },
                { Card.HatIndex, (monster, card) => monster.TrySetHat(card) },
            };

        public bool AddPart(Card card)
        {
            if (PartSetters.TryGetValue(card.BodyPartIndex, out Func<Monster, Card, bool> setterFunc))
            {
                return setterFunc(this, card);
            }
            return false;

        }

        private bool TrySetHead(Card card)
        {
            if (Head == null)
            {
                Head = card;
                return true;
            }
            return false;
        }

        private bool TrySetTorso(Card card)
        {
            if (Torso == null)
            {
                Torso = card;
                return true;
            }
            return false;
        }

        private bool TrySetLeftArm(Card card)
        {
            if (LeftArm == null && Torso != null)
            {
                LeftArm = card;
                return true;
            }
            return false;
        }

        private bool TrySetLeftHandTool(Card card)
        {
            if (LeftHandTool == null && LeftArm != null)
            {
                LeftHandTool = card;
                return true;
            }
            return false;
        }

        private bool TrySetRightArm(Card card)
        {
            if (RightArm == null && Torso != null)
            {
                RightArm = card;
                return true;
            }
            return false;
        }

        private bool TrySetRightHandTool(Card card)
        {
            if (RightHandTool == null && RightArm != null)
            {
                RightHandTool = card;
                return true;
            }
            return false;
        }

        private bool TrySetLegs(Card card)
        {
            if (Legs == null && Torso != null)
            {
                Legs = card;
                return true;
            }
            return false;
        }

        private bool TrySetHat(Card card)
        {
            if (Hat == null)
            {
                Hat = card;
                return true;
            }
            return false;
        }

        public int GetDamage()
        {
            int strength = 0;
            if (Head != null)
            {
                strength += Head.Damage;
            }

            if (Torso != null)
            {
                strength += Torso.Damage;
            }

            if (LeftArm != null)
            {
                strength += LeftArm.Damage;
            }

            if (LeftHandTool != null)
            {
                strength += LeftHandTool.Damage;
            }

            if (RightArm != null)
            {
                strength += RightArm.Damage;
            }

            if (RightHandTool != null)
            {
                strength += RightHandTool.Damage;
            }

            if (Legs != null)
            {
                strength += Legs.Damage;
            }

            if (Hat != null)
            {
                strength *= 2;
            }

            return strength;
        }

    }

}