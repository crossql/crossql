using System;

namespace crossql.Exceptions
{
    public class RepositoryCreateFailedException : Exception
    {
        public RepositoryCreateFailedException(string message) : base(message)
        {
        }
    }
}