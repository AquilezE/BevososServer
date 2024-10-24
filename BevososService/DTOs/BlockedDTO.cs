using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BevososService.DTOs
{
    [DataContract]
    public class BlockedDTO
    {
        [DataMember]
        public int BlockId { get; set; }
        [DataMember]
        public int BlockedId { get; set; }
        [DataMember]
        public string BlockerUsername { get; set; }
        [DataMember]
        public int ProfilePictureId { get; set; }
    }
}
