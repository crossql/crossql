using System;

namespace crossql.mysql
{
    public class MySqlDialect : IDialect
    {
        public DatabaseType DatabaseType => DatabaseType.MySql;
        public virtual string UseDatabase => "USE `{0}`;";

        public virtual string CreateTable => "CREATE TABLE `{0}` ({1});";

        public virtual string UpdateTable => "ALTER TABLE `{0}` ADD {1};";

        public virtual string CreateIndex => "CREATE INDEX `{0}` ON `{1}` ({2});";

        public virtual string CreateColumn => "`{0}` {1} {2}";

        public virtual string CheckDatabaseExists => "SELECT COUNT(*) AS IsExists FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{0}'";

        public virtual string CheckTableExists => "SELECT COUNT(*) AS IsExists FROM INFORMATION_SCHEMA.TABLES WHERE table_schema = 'crossql_db_dev' AND table_name = '{0}' LIMIT 1;";
        
        public virtual string CheckTableColumnExists => "SELECT COUNT(*) AS IsExists FROM sys.columns WHERE `name` = '{1}' AND `object_id` = object_id('`dbo`.`{0}`')";

        public virtual string CreateDatabase => "CREATE DATABASE `{0}`";

        public virtual string DropDatabase => "DROP DATABASE `{0}`;";

        public virtual string InsertInto => "INSERT INTO `{0}` ({1}) VALUES ({2});";

        public virtual string CreateOrUpdate => @"INSERT INTO `{0}` ({1}) VALUES ({2}) ON DUPLICATE KEY UPDATE {3}";

        public virtual string SelectFrom => "SELECT `{0}`.* FROM `{0}` {1}";

        public virtual string SelectCountFrom => "SELECT COUNT(`{0}`.*) FROM `{0}` {1}";

        public virtual string SelectMaxFrom => "SELECT MAX(`{0}`.`{2}`) FROM `{0}` {1}";

        public virtual string SelectMinFrom => "SELECT MIN(`{0}`.`{2}`) FROM `{0}` {1}";

        public virtual string SelectSumFrom => "SELECT SUM(`{0}`.`{2}`) FROM `{0}` {1}";

        public virtual string DeleteFrom => "DELETE FROM `{0}` {1}";

        public virtual string Update => "UPDATE `{0}` SET {1} {2}";

        public virtual string SelectFromJoin => "SELECT `{0}`.* FROM `{0}` {1} {2}";

        public virtual string SelectCountFromJoin => "SELECT COUNT(`0`.*) FROM `{0}` {1} {2}";

        public virtual string SelectMaxFromJoin => "SELECT MAX(`0`.`{3}`) FROM `{0}` {1} {2}";

        public virtual string SelectMinFromJoin => "SELECT MIN(`0`.`{3}`) FROM `{0}` {1} {2}";

        public virtual string SelectSumFromJoin => "SELECT SUM(`0`.`{3}`) FROM `{0}` {1} {2}";

        public virtual string DeleteFromJoin => "DELETE FROM `{0}` {1} {2}";

        public virtual string UpdateJoin => "UPDATE `{0}` SET {1} {2} {3}";

        public virtual string Where => "WHERE {0}";

        public virtual string JoinFields => "`{0}`, `{1}`";

        public virtual string JoinParameters => "@{0}, @{1}";

        public virtual string InnerJoin => "INNER JOIN `{0}` ON {1`";

        public virtual string LeftJoin => "LEFT OUTER JOIN `{0}` ON {1}";

        [Obsolete]
        public virtual string OldManyToManyJoin => "INNER JOIN `{0}` ON `{0}`.`{1}Id` = `{2}`.`Id` INNER JOIN `{3}` ON `{0}`.`{4}Id` = `{3}`.`Id`";
        
        public virtual string ManyToManyJoin => "INNER JOIN `{2}` ON `{2}`.`{3}` = `{0}`.`{1}` INNER JOIN `{4}` ON `{2}`.`{5}` = `{4}`.`{1}`";

        public virtual string SkipTake => "OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY";

        // Constraints
        public virtual string PrimaryKeyConstraint => "PRIMARY KEY";

        public virtual string ForeignKeyConstraint => ", CONSTRAINT FK_{0}_{1} FOREIGN KEY ({1}) REFERENCES {2}({3})";

        public virtual string NullableConstraint => "NULL";

        public virtual string NotNullableConstraint => "NOT NULL";

        public virtual string OnDeleteNoActionConstraint => "ON DELETE NO ACTION";

        public virtual string OnUpdateNoActionConstraint => "ON UPDATE NO ACTION";

        public virtual string UniqueConstraint => "UNIQUE";

        public virtual string DefaultBoolConstraint => "DEFAULT {0}";

        public virtual string DefaultIntegerConstraint => "DEFAULT {0}";

        public virtual string DefaultStringConstraint => "DEFAULT '{0}'";

        public virtual string CompositeKeyConstraint => "CONSTRAINT PK_{0}_{1}_Composite PRIMARY KEY {4} ({2}, {3})";

        public virtual string CompositeUniqueConstraint => "CONSTRAINT PK_{0}_{1}_Composite UNIQUE {2} ({0}, {1})";

        public virtual string ClusteredConstraint => "";

        public virtual string NonClusteredConstraint => "";

        // Data Types
        public virtual string Bool => "bit";

        public virtual string Byte => "tinyint";

        public virtual string ByteArray => "binary";

        public virtual string DateTime => "datetime";

        public virtual string DateTimeOffset => "longtext";

        public virtual string Decimal => "money";

        public virtual string Double => "float";

        public virtual string Guid => "char(36)";

        public virtual string Integer => "int";

        public virtual string Int64 => "bigint";

        public virtual string Int16 => "int";

        public virtual string LimitedString => "nvarchar({0})";

        public virtual string MaxString => "longtext";

        public virtual string Single => "real";

        public virtual string TimeSpan => "time";

        public virtual string OrderBy => "ORDER BY {0} {1}";

        public virtual string Truncate => "TRUNCATE TABLE {0}";
        
        public virtual string AutoIncrement => "AUTO_INCREMENT";
        
        public virtual string OpenBrace => "`";
        
        public virtual string CloseBrace => "`";
    }
}