using System;

namespace crossql.Attributes {
    public class TableNameAttribute : Attribute
    {
        public string Name { get; }
        public TableNameAttribute(string tableName)
        {
            Name = tableName;
        }
    }
}