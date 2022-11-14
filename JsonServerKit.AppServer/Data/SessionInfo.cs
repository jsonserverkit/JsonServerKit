namespace JsonServerKit.AppServer.Data
{
    /// <summary>
    /// Designed to signal the end of client/server communication.
    /// The server can inform the client about the session lifetime.
    /// Can also be sent before either party intends to close the connection.
    /// Purpose:
    /// -Server side tracking of the session lifetime.
    /// -Control flow between client/server.
    /// </summary>
    public class SessionInfo
    {
        /// <summary>
        /// Session lifetime valid until this datetime.
        /// </summary>
        public DateTime ValidUntil { get; set; }

        /// <summary>
        /// Flag to signal the communication end.
        /// </summary>
        public bool CloseNow { get; set; }
    }
}
