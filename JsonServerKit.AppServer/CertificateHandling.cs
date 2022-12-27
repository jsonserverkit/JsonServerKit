using System.Security.Cryptography.X509Certificates;
using JsonServerKit.AppServer.Interfaces;

namespace JsonServerKit.AppServer
{
    public class CertificateHandling : ICertificateHandling
    {
        #region Interface methods

        public X509Certificate2 GetServerCertificateFromStore(string thumbprint, StoreLocation storeLocation)
        {
            return GetCertificateFromStore(thumbprint, storeLocation);
        }

        #endregion

        public static X509Certificate2 GetCertificateFromStore(string thumbprint, StoreLocation storeLocation)
        {
            var store = new X509Store(storeLocation);
            try
            {
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                // Assuming dependency on the certificate: If the desired certificate is not found, we crash with the exception thrown by First.
                return store.Certificates.First(c => c.Thumbprint.Equals(thumbprint));
            }
            finally
            {
                store.Close();
            }
        }

        public static X509Certificate2 GetCertificateFromFile(string certificatePath, string certificatePassword)
        {
            // Certificate from file containing the server certificate.
            return new X509Certificate2(certificatePath, certificatePassword, X509KeyStorageFlags.MachineKeySet);
        }

    }
}
