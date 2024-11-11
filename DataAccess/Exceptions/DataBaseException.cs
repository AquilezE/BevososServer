using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Exceptions
{
    public class DataBaseException: Exception
    {
        public DataBaseException(string message) : base(message)
        {
        }

        public DataBaseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
