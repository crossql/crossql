﻿using System;
using System.Collections.Generic;
using crossql.Constraints;

namespace crossql
{
    public class Table
    {
        private readonly IDialect _dialect;
        private readonly bool _updateTable;

        public Table(string name, IDialect dialect, bool updateTable)
        {
            Name = name;
            Columns = new List<Column>();
            Constraints = new List<IConstraint>();
            _dialect = dialect;
            _updateTable = updateTable;
        }

        public IList<IConstraint> Constraints { get; }
        public string Name { get; }
        public IList<Column> Columns { get; }

        public Table CompositeKey(string key1, string key2, ClusterType clusterType = ClusterType.NonClustered)
        {
            Constraints.Add(new CompositeKeyConstraint(_dialect, Name, key1, key2, clusterType));
            return this;
        }

        public Table CompositeUnique(string key1, string key2, ClusterType clusterType = ClusterType.NonClustered)
        {
            Constraints.Add(new CompositeUniqueConstraint(_dialect, key1, key2, clusterType));
            return this;
        }

        public Column AddColumn(string columnName, Type dataType)
        {
            var column = AddColumn(columnName, dataType, 0);
            return column;
        }

        public Column AddColumn(string columnName, Type dataType, int precision)
        {
            var column = new Column(_dialect, columnName, dataType, Name, precision);
            Columns.Add(column);
            return column;
        }

        public override string ToString()
        {
            var ddl = "";
            if ( _updateTable )
            {
                foreach ( var column in Columns )
                {
                    ddl += Environment.NewLine + string.Format( _dialect.UpdateTable, Name, column );
                }

                // Sqlite can't added constraints after the table has already been created.
                if (_dialect.DatabaseType != DatabaseType.Sqlite)
                {
                    foreach ( var constraint in Constraints )
                    {
                        ddl += Environment.NewLine + string.Format( _dialect.UpdateTable, Name, constraint );
                    }
                }
            }
            else
            {
                var columnsAndConstraints = string.Join( ",", Columns );
                if (Constraints.Count > 0)
                {
                    columnsAndConstraints += "," + Environment.NewLine + string.Join( ",", Constraints );
                }
                ddl += Environment.NewLine + string.Format( _dialect.CreateTable, Name, columnsAndConstraints );

            }

            return ddl;
        }
    }
}