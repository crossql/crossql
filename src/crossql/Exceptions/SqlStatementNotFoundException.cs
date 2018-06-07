using System;

namespace crossql.Exceptions
{
    public class SqlStatementNotFoundException : Exception
    {
        private const string _couldNotFindFile = "Could not find the resource: '{0}'.";

        public SqlStatementNotFoundException(string fullFileName) : base(string.Format(_couldNotFindFile, fullFileName))
        {
        }
    }
}