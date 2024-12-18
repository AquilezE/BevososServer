using System.Runtime.Serialization;

namespace BevososService.DTOs
{

    [DataContract]
    public class StatsDTO
    {

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public int PointsThisGame { get; set; }

        [DataMember]
        public int AnihilatedBabies { get; set; }

        [DataMember]
        public int MonstersCreated { get; set; }

        [DataMember]
        public int Wins { get; set; }

        [DataMember]
        public bool IsWinner { get; set; }

    }

}