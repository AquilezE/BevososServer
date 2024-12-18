using System;

namespace DataAccess.Exceptions
{
    public class DataBaseException : Exception
    {
        public DataBaseException(string message) : base(message)
        {
        }

    }
}