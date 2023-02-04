﻿using Discord;
using Discord.Webhook;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Serilog.Sinks.Discord
{
    public class DiscordSink : ILogEventSink
    {
        private readonly IFormatProvider _formatProvider;
        private readonly ulong _webhookId;
        private readonly string _webhookToken;
        private readonly string? _username = null;
        private readonly LogEventLevel _restrictedToMinimumLevel;
        private readonly Dictionary<string, string> _properties;
        private readonly ulong? _threadId;

        public DiscordSink(
            IFormatProvider formatProvider,
            UInt64 webhookId,
            string webhookToken,
            string username = null,
            LogEventLevel restrictedToMinimumLevel = LogEventLevel.Information,
            Dictionary<string, string> properties = null,
            ulong? threadId = null)
        {
            _formatProvider = formatProvider;
            _webhookId = webhookId;
            _webhookToken = webhookToken;
            _username = username;
            _restrictedToMinimumLevel = restrictedToMinimumLevel;
            _properties = properties;
            _threadId = threadId;
        }

        public void Emit(LogEvent logEvent)
        {
            SendMessage(logEvent);
        }

        private void SendMessage(LogEvent logEvent)
        {
            if (!ShouldLogMessage(_restrictedToMinimumLevel, logEvent.Level))
                return;

            var embedBuilder = new EmbedBuilder();
            var webHook = new DiscordWebhookClient(_webhookId, _webhookToken);

            if (_properties != null && _properties.Any())
                foreach (var property in _properties)
                    embedBuilder.AddField(property.Key, property.Value);

            try
            {
                if (logEvent.Exception != null)
                {
                    embedBuilder.Color = new Color(255, 0, 0);
                    embedBuilder.WithTitle(":o: Exception");
                    embedBuilder.AddField("Type:", $"```{logEvent.Exception.GetType().FullName}```");

                    var message = FormatMessage(logEvent.Exception.Message, 1000);
                    embedBuilder.AddField("Message:", message);

                    var stackTrace = FormatMessage(logEvent.Exception.StackTrace, 1000);
                    embedBuilder.AddField("StackTrace:", stackTrace);
                }
                else
                {
                    var message = logEvent.RenderMessage(_formatProvider);

                    message = FormatMessage(message, 240);

                    SpecifyEmbedLevel(logEvent.Level, embedBuilder);

                    embedBuilder.Description = message;
                }

                webHook.SendMessageAsync(
                        null, false, new Embed[] { embedBuilder.Build() },
                        _username, null, null, null,
                        null, MessageFlags.None, _threadId)
                    .GetAwaiter()
                    .GetResult();
            }

            catch (Exception ex)
            {
                webHook.SendMessageAsync(
                    $"ooo snap, {ex.Message}", false, null, _username,
                    null, null , null, null,
                    MessageFlags.None, _threadId)
                    .GetAwaiter()
                    .GetResult();
            }
        }
        private static void SpecifyEmbedLevel(LogEventLevel level, EmbedBuilder embedBuilder)
        {
            switch (level)
            {
                case LogEventLevel.Verbose:
                    embedBuilder.Title = ":loud_sound: Verbose";
                    embedBuilder.Color = Color.LightGrey;
                    break;
                case LogEventLevel.Debug:
                    embedBuilder.Title = ":mag: Debug";
                    embedBuilder.Color = Color.LightGrey;
                    break;
                case LogEventLevel.Information:
                    embedBuilder.Title = ":information_source: Information";
                    embedBuilder.Color = new Color(0, 186, 255);
                    break;
                case LogEventLevel.Warning:
                    embedBuilder.Title = ":warning: Warning";
                    embedBuilder.Color = new Color(255, 204, 0);
                    break;
                case LogEventLevel.Error:
                    embedBuilder.Title = ":x: Error";
                    embedBuilder.Color = new Color(255, 0, 0);
                    break;
                case LogEventLevel.Fatal:
                    embedBuilder.Title = ":skull_crossbones: Fatal";
                    embedBuilder.Color = Color.DarkRed;
                    break;
                default:
                    break;
            }
        }

        public static string FormatMessage(string message, int maxLenght)
        {
            if (message.Length > maxLenght)
                message = $"{message.Substring(0, maxLenght)} ...";

            if (!string.IsNullOrWhiteSpace(message))
                message = $"```{message}```";

            return message;
        }

        private static bool ShouldLogMessage(
            LogEventLevel minimumLogEventLevel,
            LogEventLevel messageLogEventLevel) =>
                (int)messageLogEventLevel < (int)minimumLogEventLevel ? false : true;
    }
}
