namespace crossql.Migrations
{
    public enum MigrationStep
    {
        /// <summary>
        /// Setup code that will execute on both Server and Client 
        /// (Decided by inspecting <see cref="SystemRole"/>)
        /// </summary>
        Setup = 0,
        /// <summary>
        /// Migration code that will execute on both Server and Client 
        /// (Decided by inspecting <see cref="SystemRole"/>)
        /// </summary>
        Migrate = 1,
        /// <summary>
        /// Finalize code that will execute on both Server and Client 
        /// (Decided by inspecting <see cref="SystemRole"/>)
        /// This is typically used for seeding data or cleanup.
        /// </summary>
        Finish = 2,
        /// <summary>
        /// Setup code that will execute on the Client only
        /// (Decided by inspecting <see cref="SystemRole"/>)
        /// </summary>
        ClientSetup = 3,
        /// <summary>
        /// Migration code that will execute on the Client only
        /// (Decided by inspecting <see cref="SystemRole"/>)
        /// </summary>
        ClientMigrate = 4,
        /// <summary>
        /// Finalize code that will execute on the Client only
        /// (Decided by inspecting <see cref="SystemRole"/>)
        /// This is typically used for seeding data or cleanup.
        /// </summary>
        ClientFinish = 5,
        /// <summary>
        /// Setup code that will execute on the Server only
        /// (Decided by inspecting <see cref="SystemRole"/>)
        /// </summary>
        ServerSetup = 6,
        /// <summary>
        /// Migration code that will execute on the Server only
        /// (Decided by inspecting <see cref="SystemRole"/>)
        /// </summary>
        ServerMigrate = 7,
        /// <summary>
        /// Finalize code that will execute on the Server only
        /// (Decided by inspecting <see cref="SystemRole"/>)
        /// This is typically used for seeding data or cleanup.
        /// </summary>
        ServerFinish = 8,
        
        /// <summary>
        /// Unknown step.
        /// </summary>
        Unknown
    }
}