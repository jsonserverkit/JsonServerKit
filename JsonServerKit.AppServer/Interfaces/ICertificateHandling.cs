using System.Security.Cryptography.X509Certificates;

namespace JsonServerKit.AppServer.Interfaces
{
    public interface ICertificateHandling
    {
        public X509Certificate2 GetServerCertificateFromStore(string thumbprint, StoreLocation storeLocation);
    }
}
