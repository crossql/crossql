using Microsoft.Data.Sqlite;

namespace crossql.sqlite
{
    public class SqliteSettings
    {
        /// <summary>
        ///     Gets or sets a value that indicates whether the System.Data.Common.DbConnectionStringBuilder.ConnectionString
        ///     property is visible in Visual Studio designers. Defaults to <c>true</c>
        /// </summary>
        /// <returns>
        ///     <c>true</c> if the connection string is visible within designers; <c>false</c> otherwise. The
        ///     default is true.
        /// </returns>
        public bool BrowsableConnectionString { get; set; } = true;

        /// <summary>
        /// Gets or sets the Cache Mode used by the connection. Defaults to <see cref="SqliteCacheMode.Default"/>
        /// </summary>
        public SqliteCacheMode Cache { get; set; } = SqliteCacheMode.Default;

        /// <summary>
        /// Gets a Default instance of <see cref="SqliteSettings"/>
        /// </summary>
        public static SqliteSettings Default => new SqliteSettings();

        /// <summary>
        /// Gets or sets the connection Mode. Defaults to <see cref="SqliteOpenMode.ReadWrite"/>
        /// </summary>
        public SqliteOpenMode Mode { get; set; } = SqliteOpenMode.ReadWrite;
    }
}