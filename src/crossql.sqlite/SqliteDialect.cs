namespace crossql.sqlite
{
    public class SqliteDialect : IDialect
    {
        public DatabaseType DatabaseType => DatabaseType.Sqlite;
        public virtual string UseDatabase => "";

        public virtual string CreateTable => "CREATE TABLE [{0}] ({1});";

        public virtual string UpdateTable => "ALTER TABLE [{0}] ADD {1};";

        public virtual string CreateIndex => "CREATE INDEX [{0}] ON [{1}] ({2});";

        public virtual string CreateColumn => "[{0}] {1} {2}";

        public virtual string CheckDatabaseExists => "";

        public virtual string CheckTableExists => "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='{0}'";

        public virtual string CheckTableColumnExists => "SELECT SQL FROM sqlite_master WHERE tbl_name='{0}'";

        public virtual string CreateDatabase => "";

        public virtual string DropDatabase => "";

        public virtual string InsertInto => "INSERT INTO [{0}] ({1}) VALUES ({2})";

        public virtual string CreateOrUpdate => "INSERT OR REPLACE INTO [{0}] ({1}) VALUES ({2})";

        public virtual string SelectFrom => "SELECT [{0}].* FROM [{0}] {1}";

        public virtual string SelectCountFrom => "SELECT COUNT([{0}].*) FROM [{0}] {1}";

        public virtual string SelectMaxFrom => "SELECT MAX([{0}].[{2}]) FROM [{0}] {1}";

        public virtual string SelectMinFrom => "SELECT MIN([{0}].[{2}]) FROM [{0}] {1}";

        public virtual string SelectSumFrom => "SELECT SUM([{0}].[{2}]) FROM [{0}] {1}";

        public virtual string DeleteFrom => "DELETE FROM [{0}] {1}";

        public virtual string Update => "UPDATE [{0}] SET {1} {2}";

        public virtual string SelectFromJoin => "SELECT [{0}].* FROM [{0}] {1} {2}";

        public virtual string SelectCountFromJoin => "SELECT COUNT([0].*) FROM [{0}] {1} {2}";

        public virtual string SelectMaxFromJoin => "SELECT MAX([0].[{3}]) FROM [{0}] {1} {2}";

        public virtual string SelectMinFromJoin => "SELECT MIN([0].[{3}]) FROM [{0}] {1} {2}";

        public virtual string SelectSumFromJoin => "SELECT SUM([0].[{3}]) FROM [{0}] {1} {2}";

        public virtual string DeleteFromJoin => "DELETE FROM [{0}] {1} {2}";

        public virtual string UpdateJoin => "UPDATE [{0}] SET {1} {2} {3}";

        public virtual string Where => "WHERE {0}";

        public virtual string JoinFields => "[{0}], [{1}]";

        public virtual string JoinParameters => "@{0}, @{1}";

        public virtual string InnerJoin => "INNER JOIN [{0}] ON {1}";

        public virtual string LeftJoin => "LEFT OUTER JOIN [{0}] ON {1}";

        public virtual string OldManyToManyJoin => "INNER JOIN {0} ON {0}.{1}Id = {2}.Id INNER JOIN {3} ON {0}.{4}Id = {3}.Id";

        public virtual string ManyToManyJoin => "INNER JOIN [{2}] ON [{2}].[{3}] = [{0}].[{1}] INNER JOIN [{4}] ON [{2}].[{5}] = [{4}].[{1}]";

        public virtual string SkipTake => "LIMIT {1} OFFSET {0}";

        // Constraints
        public virtual string PrimaryKeyConstraint => "PRIMARY KEY";

        public virtual string ForeignKeyConstraint => "REFERENCES {2} ({3})";

        public virtual string NullableConstraint => "NULL";

        public virtual string NotNullableConstraint => "NOT NULL";

        public virtual string OnDeleteNoActionConstraint => "ON DELETE NO ACTION";

        public virtual string OnUpdateNoActionConstraint => "ON UPDATE NO ACTION";

        public virtual string UniqueConstraint => "UNIQUE";

        public virtual string DefaultBoolConstraint => "DEFAULT({0})";

        public virtual string DefaultIntegerConstraint => "DEFAULT({0})";

        public virtual string DefaultStringConstraint => "DEFAULT '{0}'";

        public virtual string CompositeKeyConstraint => "PRIMARY KEY ({2}, {3})";

        public virtual string CompositeUniqueConstraint => "UNIQUE ({0}, {1})";

        public virtual string ClusteredConstraint => "";

        public virtual string NonClusteredConstraint => "";

        // Data Types
        public virtual string Bool => "integer";

        public virtual string Byte => "integer";

        public virtual string ByteArray => "blob";

        public virtual string DateTime => "text";

        public virtual string DateTimeOffset => "text";

        public virtual string Decimal => "text";

        public virtual string Double => "real";

        public virtual string Guid => "blob";

        public virtual string Integer => "integer";

        public virtual string Int64 => "integer";

        public virtual string Int16 => "integer";

        public virtual string LimitedString => "text";

        public virtual string MaxString => "text";

        public virtual string Single => "real";

        public virtual string TimeSpan => "text";

        public virtual string OrderBy => "ORDER BY {0} {1}";

        public virtual string Truncate => "DELETE FROM {0}";
        
        public virtual string AutoIncrement => "";
        
        public virtual string OpenBrace => "[";
        
        public virtual string CloseBrace => "]";
    }
}