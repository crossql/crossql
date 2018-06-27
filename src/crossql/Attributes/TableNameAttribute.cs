using System;

namespace crossql.Attributes {
    
    /// <summary>
    /// Decorate a property with this attribute to indicate the Table Name to be used by the ORM
    /// </summary>
    /// <example>
    /// <![CDATA[
    ///    [TableName("Foo")]
    ///    public class FooTable
    ///    {
    ///    }
    /// ]]>
    /// </example>
    public class TableNameAttribute : Attribute
    {
        /// <summary>
        /// Actual name that you wish to use for the table
        /// </summary>
        public string Name { get; }
        public TableNameAttribute(string tableName)
        {
            Name = tableName;
        }
    }
}