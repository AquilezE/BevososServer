using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BevososService.DTOs
{
    [DataContract]
    public class StatsDTO
    {
        [DataMember]
        public string Username{ get; set; }
        [DataMember]
        public int PointsThisGame{ get; set; }
    }
}
