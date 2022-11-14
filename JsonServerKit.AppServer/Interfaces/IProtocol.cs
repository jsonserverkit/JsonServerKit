using System.Net.Security;

namespace JsonServerKit.AppServer.Interfaces
{
    public interface IProtocol
    {
        public string[] ReadMessage(SslStream sslStream);
    }
}
