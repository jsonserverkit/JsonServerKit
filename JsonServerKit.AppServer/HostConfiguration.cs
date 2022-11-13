using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JsonServerKit.AppServer
{
    public class HostConfiguration
    {
        /// <summary>
        /// Use the default but remove Microsoft logging https://learn.microsoft.com/en-us/dotnet/core/extensions/logging-providers
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args).ConfigureLogging(logging => { logging.ClearProviders(); });
    }
}
