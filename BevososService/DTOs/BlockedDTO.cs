using System.Runtime.Serialization;
using DataAccess.DAO;

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

        public static explicit operator BlockedDTO(BlockedData blockedData)
        {
            return new BlockedDTO
            {
                BlockId = blockedData.BlockId,
                BlockedId = blockedData.BlockedId,
                BlockerUsername = blockedData.BlockerUsername,
                ProfilePictureId = blockedData.ProfilePictureId
            };
        }

    }

}