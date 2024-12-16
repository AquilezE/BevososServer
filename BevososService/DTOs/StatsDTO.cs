using System.Runtime.Serialization;

namespace BevososService.DTOs
{
    [DataContract]
    public class StatsDTO
    {
        [DataMember] public string Username { get; set; }
        [DataMember] public int PointsThisGame { get; set; }
    }
}