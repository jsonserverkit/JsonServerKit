﻿namespace JsonServerKit.AppServer.Data
{
    /// <summary>
    /// Designed to initiate a session for the client/server communication.
    /// Initiated by the client.
    /// Purpose:
    /// See <see cref="SessionRequest"/>.
    /// </summary>
    public class SessionRequest
    {
        /// <summary>
        /// Property to assign some "readable" value.
        /// Generated by the client.
        /// </summary>
        public string SessionName { get; set; }
    }
}