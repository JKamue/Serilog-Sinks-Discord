using System;
using System.Collections.Generic;
using Serilog.Configuration;
using Serilog.Events;

namespace Serilog.Sinks.Discord.JKamue
{
    public static class DiscordSinkExtenstions
    {
        public static LoggerConfiguration Discord(
                this LoggerSinkConfiguration loggerConfiguration,
                UInt64 webhookId,
                string webhookToken,
                string username = null,
                IFormatProvider formatProvider = null,
                LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose,
                Dictionary<string, string> properties = null,
                ulong? threadId = null)
        {
            return loggerConfiguration.Sink(
                new DiscordSink(formatProvider, webhookId, webhookToken, username, restrictedToMinimumLevel, properties, threadId));
        }
    }
}