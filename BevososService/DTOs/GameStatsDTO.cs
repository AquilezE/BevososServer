using System.Runtime.Serialization;
using BevososService.GameModels;

namespace BevososService.DTOs
{
    [DataContract]
    public class GameStatsDTO
    {
        [DataMember]
        public int Points { get; set; }
        [DataMember]
        public int BabiesKilled { get; set; }

        public static explicit operator GameStatsDTO(Stats stats)
        {
            return new GameStatsDTO
            {
                Points = stats.PointsThisGame,
                BabiesKilled = stats.BabiesKilledThisGame
            };
        }
    }
}
