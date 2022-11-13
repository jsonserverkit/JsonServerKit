using Serilog.Core;

namespace JsonServerKit.Logging
{
    public interface ILog
    {
        public Logger GetLogger();
    }
}
