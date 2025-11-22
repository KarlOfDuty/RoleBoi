using Microsoft.Extensions.Logging;
using System;
using DSharpPlus;
using DSharpPlus.Exceptions;
using System.Threading;
using Microsoft.Extensions.Hosting.Systemd;
using Tmds.Systemd;

namespace MuteBoi;

internal class LoggerProvider : ILoggerProvider
{
    public void Dispose() { /* nothing to dispose */ }

    public ILogger CreateLogger(string categoryName)
    {
        return new Logger(categoryName);
    }
}

public class Logger(string logCategory) : ILogger
{
    public static Logger Instance { get; } = new Logger("MuteBoi");

    private static LogLevel minimumLogLevel = LogLevel.Trace;
    private static readonly Lock consoleLock = new();
    private bool IsSingleton => logCategory == "MuteBoi";

    private static readonly EventId botEventId = new EventId(420, "BOT");

    internal static void SetLogLevel(LogLevel level)
    {
        minimumLogLevel = level;
    }

    internal static void Debug(string message, Exception exception = null)
    {
        Instance.Log(LogLevel.Debug, botEventId, exception, message);
    }

    internal static void Log(string message, Exception exception = null)
    {
        Instance.Log(LogLevel.Information, botEventId, exception, message);
    }

    internal static void Warn(string message, Exception exception = null)
    {
        Instance.Log(LogLevel.Warning, botEventId, exception, message);
    }

    internal static void Error(string message, Exception exception = null)
    {
        Instance.Log(LogLevel.Error, botEventId, exception, message);
    }

    internal static void Fatal(string message, Exception exception = null)
    {
        Instance.Log(LogLevel.Critical, botEventId, exception, message);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= minimumLogLevel && logLevel != LogLevel.None;
    }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => default;

    private static ConsoleColor GetLogLevelColour(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace       => ConsoleColor.White,
            LogLevel.Debug       => ConsoleColor.DarkGray,
            LogLevel.Information => ConsoleColor.DarkBlue,
            LogLevel.Warning     => ConsoleColor.Yellow,
            LogLevel.Error       => ConsoleColor.Red,
            _                    => ConsoleColor.White
        };
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        string message = formatter(state, exception);

        // Ratelimit messages are usually warnings, but they are unimportant in this case so downgrade them to debug.
        if (message.StartsWith("Hit Discord ratelimit on route ") && logLevel == LogLevel.Warning)
        {
            logLevel = LogLevel.Debug;
        }
        // The bot will handle NotFoundExceptions on its own, downgrade to debug
        else if (exception is NotFoundException && eventId == LoggerEvents.RestError)
        {
            logLevel = LogLevel.Debug;
        }

        // Uncomment to check log category of log message
        //Console.WriteLine("Log Category: " + logCategory);

        // Remove HTTP Client spam
        if (logCategory.StartsWith("System.Net.Http.HttpClient"))
        {
            return;
        }

        LogToConsoleOrSystemd(logLevel, message, exception);
    }

    private void LogToConsoleOrSystemd(LogLevel logLevel, string message, Exception exception)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        if (SystemdHelpers.IsSystemdService())
        {
            SystemdLog(logLevel, exception, message);
        }
        else
        {
            ConsoleLog(logLevel, exception, message);
        }
    }

    private void SystemdLog(LogLevel logLevel, Exception exception, string message)
    {
        string logLevelTag = logLevel switch
        {
            LogLevel.Trace       => "[Trace] ",
            LogLevel.Debug       => "[Debug] ",
            LogLevel.Information => " [Info] ",
            LogLevel.Warning     => " [Warn] ",
            LogLevel.Error       => "[Error] ",
            LogLevel.Critical    => " [Crit] ",
            _                    => " [None] ",
        };

        LogFlags priority = logLevel switch
        {
            LogLevel.Trace       => LogFlags.Debug,
            LogLevel.Debug       => LogFlags.Debug,
            LogLevel.Information => LogFlags.Information,
            LogLevel.Warning     => LogFlags.Warning,
            LogLevel.Error       => LogFlags.Error,
            LogLevel.Critical    => LogFlags.Critical,
            _                    => LogFlags.Information
        };

        string logMessage = (IsSingleton ? "[BOT] " : "[API] ") + logLevelTag + message;
        if (exception != null)
        {
            logMessage += "\n" + GetExceptionString(exception, 0);
        }

        JournalMessage msg = Journal.GetMessage().Append(JournalFieldName.Message, logMessage);
        if (Journal.IsAvailable)
        {
            Journal.Log(priority, msg);
        }
        else
        {
            Console.WriteLine(logMessage);
        }
    }

    private void ConsoleLog(LogLevel logLevel, Exception exception, string message)
    {
        string[] logLevelParts = logLevel switch
        {
            LogLevel.Trace       => ["[", "Trace", "] "],
            LogLevel.Debug       => ["[", "Debug", "] "],
            LogLevel.Information => [" [", "Info", "] "],
            LogLevel.Warning     => [" [", "Warn", "] "],
            LogLevel.Error       => ["[", "Error", "] "],
            LogLevel.Critical    => [" [", "\e[1mCrit\e[0m", "] "],
            _                    => [" [", "None", "] "],
        };

        using Lock.Scope _ = consoleLock.EnterScope();

        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write("[");

        Console.ResetColor();
        Console.ForegroundColor = GetLogLevelColour(logLevel);
        if (logLevel == LogLevel.Critical)
        {
            Console.BackgroundColor = ConsoleColor.DarkRed;
        }
        Console.Write($"{DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}");
        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write("] ");

        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write("[");

        Console.ForegroundColor = IsSingleton ? ConsoleColor.Green : ConsoleColor.DarkGreen;
        Console.Write(IsSingleton ? "BOT" : "API");

        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write("] ");
        Console.Write(logLevelParts[0]);

        Console.ForegroundColor = GetLogLevelColour(logLevel);
        if (logLevel == LogLevel.Critical)
        {
            Console.BackgroundColor = ConsoleColor.DarkRed;
        }
        Console.Write(logLevelParts[1]);

        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write(logLevelParts[2]);

        Console.ResetColor();
        if (logLevel is LogLevel.Trace or LogLevel.Debug)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
        }
        else if (logLevel is LogLevel.Critical or LogLevel.Error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
        }
        Console.WriteLine(message);

        if (exception != null)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(GetExceptionString(exception, 0));
        }

        Console.ResetColor();
    }

    private static string GetExceptionString(Exception exception, int indentation = 0)
    {
        string exceptionString = $"{new string(' ', indentation)}{exception}: {exception.Message}";

        // Add stack trace if it is not included in the message
        if (exception.StackTrace != null && !exceptionString.Contains(exception.StackTrace))
        {
            exceptionString += $"\n{exception.StackTrace}";
        }

        return exceptionString.Replace("\n", "\n" + new string(' ', indentation));
    }
}