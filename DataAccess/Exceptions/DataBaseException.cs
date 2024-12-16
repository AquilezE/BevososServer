using System;

namespace DataAccess.Exceptions
{
    public class DataBaseException : Exception
    {
        public DataBaseException(string message) : base(message)
        {
        }

        public DataBaseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}