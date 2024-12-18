using System.Runtime.Serialization;
using DataAccess.DAO;

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

        public static explicit operator FriendRequestDTO(FriendRequestData friendRequestData)
        {
            return new FriendRequestDTO
            {
                FriendRequestId = friendRequestData.FriendRequestId,
                SenderId = friendRequestData.SenderId,
                SenderName = friendRequestData.SenderName,
                ProfilePictureId = friendRequestData.ProfilePictureId
            };
        }

    }

}