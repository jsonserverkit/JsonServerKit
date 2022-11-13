namespace JsonServerKit.AppServer.Interfaces
{
    public interface ITcpServerConfig
    {
        public int Port { get; set; }
        public string CertificatePath { get; set; }
        public string CertificatePassword { get; set; }
        public string CertificateThumbprint { get; set; }
    }
}
