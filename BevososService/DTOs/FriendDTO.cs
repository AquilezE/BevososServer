using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;


namespace BevososService.DTOs
{
    [DataContract]
    public class FriendDTO
    {
        [DataMember]
        public int FriendshipId { get; set; }
        [DataMember]
        public int FriendId { get; set; }
        [DataMember]
        public string FriendName { get; set; }
        [DataMember]
        public int ProfilePictureId { get; set; }
        [DataMember]
        public bool IsConnected {  get; set; } = false;
    }
}
