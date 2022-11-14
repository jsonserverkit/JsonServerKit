namespace JsonServerKit.AppServer.Interfaces
{
    public interface ITcpServerConfig
    {
        /// <summary>
        /// Defines the server hosting port number.
        /// </summary>
        public int Port { get; set; }

        public string CertificatePath { get; set; }
        public string CertificatePassword { get; set; }

        public string CertificateThumbprint { get; set; }
        /// <summary>
        /// 0 means infinite. 0 < Means time in milliseconds.
        /// </summary>
        public int ReceiveTimeout { get; set; }

        /// <summary>
        /// 0 means infinite 0 < Means time in milliseconds.
        /// </summary>
        public int SendTimeout { get; set; }

        /// <summary>
        /// Speaks for it self.
        /// </summary>
        public bool RequireClientAuthentication { get; set; }

        /// <summary>
        /// Normally, the value of this property should be set to true (the default) for security reasons,
        /// but there are times when it may be necessary to set it to false.
        ///
        /// For example, most Certificate Authorities are probably pretty good at keeping their CRL and/or OCSP servers up 24/7, 
        /// but occasionally they do go down or are otherwise unreachable due to other network problems between the client and the Certificate Authority. 
        /// When this happens, it becomes impossible to check the revocation status of one or more of the certificates in the chain
        /// resulting in an SslHandshakeException being thrown in the Connect method.
        /// If this becomes a problem, it may become desirable to set CheckCertificateRevocation to false.
        /// </summary>
        public bool CheckCertificateRevocation { get; set; }

    }
}
