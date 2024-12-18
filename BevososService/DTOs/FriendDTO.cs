using System.Runtime.Serialization;
using DataAccess.DAO;


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
        public bool IsConnected { get; set; }

        public static explicit operator FriendDTO(FriendData friendData)
        {
            return new FriendDTO
            {
                FriendshipId = friendData.FriendshipId,
                FriendId = friendData.FriendId,
                FriendName = friendData.FriendName,
                ProfilePictureId = friendData.ProfilePictureId,
                IsConnected = friendData.IsConnected
            };
        }

    }

}