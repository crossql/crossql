using System;

namespace crossql.mssqlserver
{
    public class SqlServerDialect : IDialect
    {
        public DatabaseType DatabaseType => DatabaseType.SqlServer;
        public virtual string UseDatabase => "USE [{0}];";

        public virtual string CreateTable => "CREATE TABLE [{0}] ({1});";

        public virtual string UpdateTable => "ALTER TABLE [{0}] ADD {1};";

        public virtual string CreateIndex => "CREATE INDEX [{0}] ON [{1}] ({2});";

        public virtual string CreateColumn => "[{0}] {1} {2}";

        public virtual string CheckDatabaseExists => "SELECT COUNT(*) AS IsExists FROM sys.databases WHERE Name = '{0}'";

        public virtual string CheckTableExists => "SELECT COUNT(*) AS IsExists FROM dbo.sysobjects WHERE id = object_id('[dbo].[{0}]')";

        public virtual string CheckTableColumnExists => "SELECT COUNT(*) AS IsExists FROM sys.columns WHERE [name] = '{1}' AND [object_id] = object_id('[dbo].[{0}]')";

        public virtual string CreateDatabase => "CREATE DATABASE [{0}] ALTER DATABASE [{0}] MODIFY FILE ( NAME = N'{0}' , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB ) ALTER DATABASE [{0}] MODIFY FILE ( NAME = N'{0}_log' , MAXSIZE = 1024GB , FILEGROWTH = 10%)";

        public virtual string DropDatabase => "ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{0}];";

        public virtual string InsertInto => "INSERT INTO [{0}] ({1}) VALUES ({2});";

        // 0 == tablename
        // 1 = set
        // 2 = where
        // 3 = fields
        // 4 = parameters
        public virtual string CreateOrUpdate => @"UPDATE [{0}] SET {1} {2};
IF @@ROWCOUNT = 0
BEGIN;
    INSERT INTO [{0}] ({3}) VALUES ({4});
END;";

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

        [Obsolete]
        public virtual string OldManyToManyJoin => "INNER JOIN [{0}] ON [{0}].[{1}Id] = [{2}].[Id] INNER JOIN [{3}] ON [{0}].[{4}Id] = [{3}].[Id]";
        
        public virtual string ManyToManyJoin => "INNER JOIN [{2}] ON [{2}].[{3}] = [{0}].[{1}] INNER JOIN [{4}] ON [{2}].[{5}] = [{4}].[{1}]";

        public virtual string SkipTake => "OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY";

        // Constraints
        public virtual string PrimaryKeyConstraint => "PRIMARY KEY";

        public virtual string ForeignKeyConstraint => "CONSTRAINT FK_{0}_{1} FOREIGN KEY ({1}) REFERENCES {2} ({3})";

        public virtual string NullableConstraint => "NULL";

        public virtual string NotNullableConstraint => "NOT NULL";

        public virtual string OnDeleteNoActionConstraint => "ON DELETE NO ACTION";

        public virtual string OnUpdateNoActionConstraint => "ON UPDATE NO ACTION";

        public virtual string UniqueConstraint => "UNIQUE";

        public virtual string DefaultBoolConstraint => "DEFAULT({0})";

        public virtual string DefaultIntegerConstraint => "DEFAULT({0})";

        public virtual string DefaultStringConstraint => "DEFAULT '{0}'";

        public virtual string CompositeKeyConstraint => "CONSTRAINT PK_{0}_{1}_Composite PRIMARY KEY {4} ({2}, {3})";

        public virtual string CompositeUniqueConstraint => "CONSTRAINT PK_{0}_{1}_Composite UNIQUE {2} ({0}, {1})";

        public virtual string ClusteredConstraint => "CLUSTERED";

        public virtual string NonClusteredConstraint => "NONCLUSTERED";

        // Data Types
        public virtual string Bool => "bit";

        public virtual string Byte => "tinyint";

        public virtual string ByteArray => "binary";

        public virtual string DateTime => "datetime";

        public virtual string DateTimeOffset => "datetimeoffset";

        public virtual string Decimal => "money";

        public virtual string Double => "float";

        public virtual string Guid => "uniqueidentifier";

        public virtual string Integer => "int";

        public virtual string Int64 => "bigint";

        public virtual string Int16 => "int";

        public virtual string LimitedString => "nvarchar({0})";

        public virtual string MaxString => "nvarchar(max)";

        public virtual string Single => "real";

        public virtual string TimeSpan => "time";

        public virtual string OrderBy => "ORDER BY {0} {1}";

        public virtual string Truncate => "TRUNCATE TABLE {0}";
        
        public virtual string AutoIncrement => "IDENTITY({0},{1})";
        
        public virtual string OpenBrace => "[";
        
        public virtual string CloseBrace => "]";
    }
}