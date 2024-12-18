using System.Runtime.Serialization;

namespace BevososService.Exceptions
{

    [DataContract]
    public class BevososServerExceptions
    {

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public string StackTrace { get; set; }

    }

}