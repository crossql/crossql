namespace crossql
{
    public enum SystemRole
    {
        /// <summary>
        /// Indicates to the system that it's being run on a Client (typically sqlite)
        /// </summary>
        Client,
        /// <summary>
        /// Indicates to the system that it's being run on a Server
        /// </summary>
        Server
    }
}