using System;
using crossql.Attributes;

namespace crossql.Models
{
    [TableName("__version")]
    public class Version
    {
        public int VersionNumber { get; set; }
        public bool IsSetupComplete { get; set; }
        public bool IsMigrationComplete { get; set; }
        public bool IsFinishComplete { get; set; }
        public DateTimeOffset MigrationDate { get; set; }
    }
}