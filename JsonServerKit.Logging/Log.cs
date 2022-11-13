using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Formatting.Json;

namespace JsonServerKit.Logging
{
    public class Log: ILog
    {
        #region Log configuration

        public class LogConfig : ILogConfig
        {
            public string PathLogFileJsonFormated { get; set; }
            public string PathLogFileTextFormated { get; set; }
        }

        #endregion

        #region Private members

        private readonly IConfiguration _configuration;
        private readonly ILogConfig _logConfig;

        #endregion

        #region Constructors

        public Log(IConfiguration configuration, ILogConfig logConfig)
        {
            _configuration = configuration;
            _logConfig = logConfig;
        }

        #endregion

        #region Interface methods

        public Logger GetLogger()
        {
            // Dynamic Config to File 
            // https://stackoverflow.com/questions/72019128/serilog-enrichers-dont-log-to-console-net-6-0
            // Logging with Expressions
            // https://nblumhardt.com/2021/06/customize-serilog-text-output/#-listing-only-hidden-properties-not-otherwise-present-in-the-message-or-template
            // Extended Properties Logging Features
            // https://stackoverflow.com/questions/49090613/c-sharp-serilog-enrichers-leaving-blank-entries
            // String Alignment
            // https://nblumhardt.com/2017/06/serilog-2-5/
            return GetBasicLogConfiguration(_configuration, _logConfig.PathLogFileJsonFormated, _logConfig.PathLogFileTextFormated).CreateLogger();
        }

        #endregion

        #region Private methods

        private LoggerConfiguration GetBasicLogConfiguration(IConfiguration configuration, string logPathJson, string logPathPlain)
        {
            var outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff}|{MachineName}|{ProcessId:D6}|{ThreadId:D4}|[{Level:u4}]|{Message:l}{NewLine}{Exception}";
            return new LoggerConfiguration().ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .Enrich.WithProcessName()
                .WriteTo.File(new JsonFormatter(), path: logPathJson)
                .WriteTo.File(path: logPathPlain, outputTemplate: outputTemplate);
        }

        private LoggerConfiguration GetExtendedLogConfiguration(IConfiguration configuration, string logPathJson, string logPathPlain)
        {
            var outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff}|{MachineName}|{EnvironmentName,-16:u4}|{EnvironmentUserName}|{ProcessName,16:lj}|{ProcessId:D6}|{ThreadId:D4}|[{Level:u4}]|{Message:l}{NewLine}{Exception}";
            return new LoggerConfiguration().ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithEnvironmentUserName()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .Enrich.WithProcessName()
                .WriteTo.File(new JsonFormatter(), path: logPathJson)
                .WriteTo.File(path: logPathPlain, outputTemplate: outputTemplate);
        }

        #endregion
    }
    }