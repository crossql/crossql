using System;

namespace crossql.Attributes
{
    /// <summary>
    /// Decorate an <see cref="int"/> or <see cref="long"/> property with this attribute to indicate that it's an AutoIncrement field in the database
    /// </summary>
    public class AutoIncrement : Attribute { }
}