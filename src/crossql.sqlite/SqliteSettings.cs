using Microsoft.Data.Sqlite;

namespace crossql.sqlite
{
    public class SqliteSettings
    {
        public static SqliteSettings Default => new SqliteSettings();

        public bool EnforceForeignKeys { get; set; } = true;
        public SqliteCacheMode Cache { get; set; } = SqliteCacheMode.Default;
        public SqliteOpenMode Mode { get; set; } = SqliteOpenMode.ReadWrite;
        public bool BrowsableConnectionString { get; set; } = true;

        // todo: <Chase Florell: June 10, 2018> figure out what to do with these. They're
        // todo: <Chase Florell: June 10, 2018> no longer supported in Microsoft.Data.Sqlite
        // todo: <Chase Florell: June 10, 2018> https://github.com/aspnet/Microsoft.Data.Sqlite/issues/309#issuecomment-267464112
        //// Default Values
        ////-----------------------------
        //private int _pageSize = 1024;
        ////-----------------------------

        //public SQLiteJournalModeEnum JournalMode { get; set; } = SQLiteJournalModeEnum.Delete;

        //public int PageSize
        //{
        //    get => _pageSize;
        //    set
        //    {
        //        if (value > 65536)
        //        {
        //            CacheSize = 65536;
        //        }
        //        else
        //        {
        //            _pageSize = value;
        //        }
        //    }
        //}

        //public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(0.1);

        //public SynchronizationModes SynchronizationModes { get; set; } = SynchronizationModes.Normal;

        //public int CacheSize { get; set; } = 2000;

        //public bool FailIfMissing { get; set; } = true;

        //public bool ReadOnly { get; set; }
    }

    //public enum SynchronizationModes
    //{
    //    Full,
    //    Normal,
    //    Off
    //}

    //public enum SQLiteJournalModeEnum
    //{
    //    Delete,
    //    Off,
    //    Persist
    //}
}