using JsonServerKit.AppServer.Data;

namespace JsonServerKit.AppServer.Interfaces
{
    public interface ITcpSession
    {
        #region Interface methods

        public Task<bool> AuthenticateServerAsync(CancellationToken stoppingToken);
        public void StartReceiving(CancellationToken stoppingToken);
        public void StartSending(CancellationToken stoppingToken);
        public void HandleSessionRequest(dynamic incommingMsg, string msg, ReceiveSendContext receiveSendContext);  
        public void HandlePayload(dynamic incommingMsg, string msg, ReceiveSendContext receiveSendContext);
        public bool HandleSessionInfo(dynamic incommingMsg, string msg);

        #endregion
    }
}
