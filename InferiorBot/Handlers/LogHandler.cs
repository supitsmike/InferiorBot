using Discord;
using MediatR;
using Serilog;
using Serilog.Events;

namespace InferiorBot.Handlers
{
    public class LogNotification(LogMessage message) : INotification
    {
        public LogMessage Message { get; } = message;
    }

    public class LogHandler : INotificationHandler<LogNotification>
    {
        public Task Handle(LogNotification notification, CancellationToken cancellationToken)
        {
            var message = notification.Message;
            var severity = message.Severity switch
            {
                LogSeverity.Critical => LogEventLevel.Fatal,
                LogSeverity.Error => LogEventLevel.Error,
                LogSeverity.Warning => LogEventLevel.Warning,
                LogSeverity.Info => LogEventLevel.Information,
                LogSeverity.Verbose => LogEventLevel.Verbose,
                LogSeverity.Debug => LogEventLevel.Debug,
                _ => LogEventLevel.Information
            };

            Log.Write(severity, message.Exception, "[{Source}] {Message}", message.Source, message.Message);
            return Task.CompletedTask;
        }
    }
}
