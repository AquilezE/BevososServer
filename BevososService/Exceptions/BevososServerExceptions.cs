using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
