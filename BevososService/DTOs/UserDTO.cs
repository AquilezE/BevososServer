using System.Runtime.Serialization;

namespace BevososService.DTOs
{

    [DataContract]
    public class UserDTO
    {
        [DataMember]
        public int UserId { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public int ProfilePictureId { get; set; }

        [DataMember]
        public bool IsReady { get; set; } = false;

        public static explicit operator UserDTO(DataAccess.Models.User user)
        {
            return new UserDTO
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Account.Email,
                ProfilePictureId = user.ProfilePictureId

            };
        }
    }
}
