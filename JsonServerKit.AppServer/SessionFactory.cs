using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using JsonServerKit.AppServer.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace JsonServerKit.AppServer
{
    public class SessionFactory
    {
        public static Func<IServiceProvider, Func<TcpClient, X509Certificate2, ITcpSession>> GetTcpSession =>
            service =>
            {
                return
                    (tcpClient, x509Certificate2) =>
                    {
                        var msgProcessor = service.GetRequiredService<IMessageProcessor>();
                        var logger = service.GetRequiredService<ILogger>();
                        var protocol = service.GetRequiredService<IProtocol>();
                        return new TcpSession(tcpClient, msgProcessor, x509Certificate2, logger, protocol);
                    };
            };
    }
}
