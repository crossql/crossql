using System;

namespace crossql.Exceptions
{
    public class ConstraintException : Exception
    {
        public ConstraintException(string message) : base(message)
        {
            
        }
    }
}