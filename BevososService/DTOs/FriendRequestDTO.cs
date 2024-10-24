using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace BevososService.DTOs
{
    [DataContract]
    public class FriendRequestDTO
    {
        [DataMember]
        public int FriendRequestId { get; set; }
        [DataMember]
        public int SenderId { get; set; }
        [DataMember]
        public string SenderName { get; set; }
        [DataMember]
        public int ProfilePictureId { get; set; }
    }
}
