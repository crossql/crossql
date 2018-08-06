using System;
using System.Collections.Generic;
using System.Linq;
using crossql.Constraints;
using crossql.Exceptions;
using ForeignKeyConstraint = crossql.Constraints.ForeignKeyConstraint;
using UniqueConstraint = crossql.Constraints.UniqueConstraint;

namespace crossql
{
    public class Column
    {
        public static IList<KeyValuePair<Type, string>> CustomTypes = new List<KeyValuePair<Type, string>>();
        public readonly IList<IConstraint> Constraints;
        private readonly IList<IConstraint> _firstConstraints;
        public readonly string Name;
        public readonly int Precision;
        public Type Type;
        private readonly IDialect _dialect;

        private readonly string _tableName;

        public Column(IDialect dialect, string name, Type type, string tableName, int precision)
        {
            Constraints = new List<IConstraint>();
            _firstConstraints = new List<IConstraint>();
            Name = name;
            Precision = precision;
            Type = type;
            _dialect = dialect;
            _tableName = tableName;
        }

        public Column AsCustomType(string dialectValue)
        {
            CustomTypes.Add(new KeyValuePair<Type, string>(Type, dialectValue));
            return this;
        }

        public Column AutoIncrement(int start, int increment)
        {
            if (_dialect.DatabaseType == DatabaseType.Sqlite)
            {
                // sqlite requires autoincrementing fields to be of type 'long', 
                // so we're going to be a little helpful and swapping the it on the fly.
                if (Type == typeof(int) || Type == typeof(byte) || Type == typeof(short))
                {
                    Type = typeof(long);
                }
            }
            if (Type != typeof(int) && Type != typeof(byte) && Type != typeof(short) && Type != typeof(long))
            {
                throw new ConstraintException("Auto Incrementing fileds must be of one of the following types [byte, short, int, long]");
            }
            Constraints.Add(new AutoIncrementConstraint(_dialect, start, increment));
            return this;
        }

        public Column Clustered()
        {
            _firstConstraints.Add(new ClusteredConstraint(_dialect));
            return this;
        }

        public Column Default<T>(T defaultValue)
        {
            Constraints.Add(new DefaultConstraint<T>(_dialect, defaultValue));
            return this;
        }

        /// <summary>
        ///     Creates a Foreign Key Constraint.
        /// </summary>
        /// <param name="referenceTable">The reference table.</param>
        /// <param name="referenceField">The reference field.</param>
        /// <returns>Column.</returns>
        public Column ForeignKey(string referenceTable, string referenceField)
        {
            Constraints.Add(new ForeignKeyConstraint(_dialect, _tableName, Name, referenceTable, referenceField));
            Constraints.Add(new OnDeleteNoActionConstraint(_dialect));
            Constraints.Add(new OnUpdateNoActionConstraint(_dialect));
            return this;
        }

        public Column NonClustered()
        {
            _firstConstraints.Add(new NonClusteredConstraint(_dialect));
            return this;
        }

        public Column NotNullable()
        {
            _firstConstraints.Add(new NotNullableConstraint(_dialect));
            return this;
        }

        public Column NotNullable<T>(T defaultValue)
        {
            _firstConstraints.Add(new NotNullableConstraint(_dialect));
            _firstConstraints.Add(new DefaultConstraint<T>(_dialect, defaultValue));
            return this;
        }

        public Column Nullable()
        {
            _firstConstraints.Add(new NullableConstraint(_dialect));
            return this;
        }

        public Column PrimaryKey()
        {
            _firstConstraints.Add(new PrimaryKeyConstraint(_dialect));
            return this;
        }

        public override string ToString()
        {
            return Environment.NewLine +
                   string.Format(_dialect.CreateColumn, Name, GetDataType(Type, Precision),
                       string.Join(" ", _firstConstraints.Union(Constraints)));
        }

        public Column Unique()
        {
            Constraints.Add(new UniqueConstraint(_dialect));
            return this;
        }

        private string GetDataType(Type type, int precision)
        {
            if (type == typeof(string) && precision == 0)
                return _dialect.MaxString;

            if (type == typeof(string))
                return string.Format(_dialect.LimitedString, precision);

            if (type == typeof(int))
                return _dialect.Integer;

            if (type == typeof(short))
                return _dialect.Int16;

            if (type == typeof(DateTime))
                return _dialect.DateTime;

            if (type == typeof(DateTimeOffset))
                return _dialect.DateTimeOffset;

            if (type == typeof(Guid))
                return _dialect.Guid;

            if (type == typeof(bool))
                return _dialect.Bool;

            if (type == typeof(decimal))
                return _dialect.Decimal;

            if (type == typeof(byte))
                return _dialect.Byte;

            if (type == typeof(long))
                return _dialect.Int64;

            if (type == typeof(double) || type == typeof(float))
                return _dialect.Double;

            if (type == typeof(byte[]))
                return _dialect.ByteArray;

            if (type == typeof(float))
                return _dialect.Single;

            if (type == typeof(TimeSpan))
                return _dialect.TimeSpan;

            foreach (var customType in CustomTypes)
                if (type == customType.Key)
                    return customType.Value;

            throw new DataTypeNotSupportedException();
        }
    }
}