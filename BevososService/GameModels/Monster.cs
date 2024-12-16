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

        public bool AddPart(Card card)
        {
            switch (card.BodyPartIndex)
            {
                case 0:
                    if (Head == null)
                    {
                        Head = card;
                        return true;
                    }

                    break;
                case 1:
                    if (Torso == null)
                    {
                        Torso = card;
                        return true;
                    }

                    break;
                case 2:
                    if (LeftArm == null && Torso != null)
                    {
                        LeftArm = card;
                        return true;
                    }

                    break;
                case 3:
                    if (LeftHandTool == null && LeftArm != null)
                    {
                        LeftHandTool = card;
                        return true;
                    }

                    break;
                case 4:
                    if (RightArm == null && Torso != null)
                    {
                        RightArm = card;
                        return true;
                    }

                    break;
                case 5:
                    if (RightHandTool == null && RightArm != null)
                    {
                        RightHandTool = card;
                        return true;
                    }

                    break;
                case 6:
                    if (Legs == null && Torso != null)
                    {
                        Legs = card;
                        return true;
                    }

                    break;
                case 7:
                    if (Hat == null)
                    {
                        Hat = card;
                        return true;
                    }

                    break;

                default:
                    return false;
            }

            return false;
        }

        public int GetDamage()
        {
            int strength = 0;
            if (Head != null) strength += Head.Damage;
            if (Torso != null) strength += Torso.Damage;
            if (LeftArm != null) strength += LeftArm.Damage;
            if (LeftHandTool != null) strength += LeftHandTool.Damage;
            if (RightArm != null) strength += RightArm.Damage;
            if (RightHandTool != null) strength += RightHandTool.Damage;
            if (Legs != null) strength += Legs.Damage;
            if (Hat != null) strength = strength * 2;

            return strength;
        }
    }
}