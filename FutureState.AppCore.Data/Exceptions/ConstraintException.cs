using System;

namespace FutureState.AppCore.Data.Exceptions
{
    public class ConstraintException : Exception
    {
        public ConstraintException(string message) : base(message)
        {
            
        }
    }
}