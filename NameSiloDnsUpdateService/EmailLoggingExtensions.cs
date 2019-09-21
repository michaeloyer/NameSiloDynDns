using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.Email;
using System;
using System.Net;

namespace NameSiloDnsUpdateService
{
    public static class EmailLoggingExtensions
    {
        public static LoggerConfiguration Email(this LoggerSinkConfiguration loggerConfiguration, MyEmailConnectionInfo myConnectionInfo, string outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}", LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose, int batchPostingLimit = 100, TimeSpan? period = null, IFormatProvider formatProvider = null, string mailSubject = "Log Email") =>
            loggerConfiguration.Email((EmailConnectionInfo)myConnectionInfo, outputTemplate, restrictedToMinimumLevel, batchPostingLimit, period, formatProvider, mailSubject);
        public static LoggerConfiguration Email(this LoggerSinkConfiguration loggerConfiguration, MyEmailConnectionInfo myConnectionInfo, ITextFormatter textFormatter, LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose, int batchPostingLimit = 100, TimeSpan? period = null, string mailSubject = "Log Email") =>
            loggerConfiguration.Email((EmailConnectionInfo)myConnectionInfo, textFormatter, restrictedToMinimumLevel, batchPostingLimit, period, mailSubject);

        public class MyEmailConnectionInfo : EmailConnectionInfo
        {
            public new NetworkCredential NetworkCredentials
            {
                get => (NetworkCredential)base.NetworkCredentials;
                set => base.NetworkCredentials = value;
            }
        }
    }
}
