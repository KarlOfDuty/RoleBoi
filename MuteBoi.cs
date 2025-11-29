using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Channels;
using DSharpPlus;
using CommandLine;
using DSharpPlus.Entities;
using Microsoft.Extensions.Hosting.Systemd;
using Microsoft.Extensions.Logging;
using Tmds.Systemd;
using ServiceState = Tmds.Systemd.ServiceState;

namespace MuteBoi;

internal static class MuteBoi
{
    internal static DiscordClient client = null;

    internal const string APPLICATION_NAME = "MuteBoi";

    private static Timer statusUpdateTimer;

    public class CommandLineArguments
    {
        [Option('c',
                "config",
                Required = false,
                HelpText = "Select a config file to use.",
                MetaValue = "PATH")]
        public string configPath { get; set; }

        [Option('l',
            "log-file",
            Required = false,
            HelpText = "Select log file to write bot logs to.",
            MetaValue = "PATH")]
        public string logFilePath { get; set; }

        [Option("leave",
                Required = false,
                HelpText = "Leaves one or more Discord servers. " +
                           "You can check which servers your bot is in when it starts up.",
                MetaValue = "ID,ID,ID...",
                Separator = ','
        )]
        public IEnumerable<ulong> serversToLeave { get; set; }
    }

    internal static CommandLineArguments commandLineArgs;

    private static readonly Channel<PosixSignal> signalChannel = Channel.CreateUnbounded<PosixSignal>();

    private static void HandleSignal(PosixSignalContext context)
    {
        context.Cancel = true;
        signalChannel.Writer.TryWrite(context.Signal);
    }

    // ServiceManager will steal this value later so we have to copy it while we have the chance.
    private static readonly string systemdSocket = Environment.GetEnvironmentVariable("NOTIFY_SOCKET");

    private static async Task<int> Main(string[] args)
    {
        if (SystemdHelpers.IsSystemdService())
        {
            Journal.SyslogIdentifier = Assembly.GetEntryAssembly()?.GetName().Name;
            PosixSignalRegistration.Create(PosixSignal.SIGHUP, HandleSignal);
        }

        PosixSignalRegistration.Create(PosixSignal.SIGTERM, HandleSignal);
        PosixSignalRegistration.Create(PosixSignal.SIGINT, HandleSignal);

        StringWriter sw = new();
        commandLineArgs = new Parser(settings =>
        {
            settings.AutoHelp = true;
            settings.HelpWriter = sw;
            settings.AutoVersion = false;
        }).ParseArguments<CommandLineArguments>(args).Value;

        // CommandLineParser has some bugs related to the built-in version option, ignore the output if it isn't found.
        if (!sw.ToString().Contains("Option 'version' is unknown."))
        {
            Console.Write(sw);
        }

        if (args.Contains("--help"))
        {
            return 0;
        }

        if (args.Contains("--version"))
        {
            Console.WriteLine(APPLICATION_NAME + ' ' + GetVersion());
            Console.WriteLine("Build time: " + BuildInfo.BuildTimeUTC.ToString("yyyy-MM-dd HH:mm:ss") + " UTC");
            return 0;
        }

        Logger.Log("Starting " + APPLICATION_NAME + " version " + GetVersion() + "...");
        try
        {
            if (!Reload())
            {
                Logger.Fatal("Aborting startup due to a fatal error when loading the configuration and setting up the database.");
                return 1;
            }

            // Create but don't start the timer, it will be started when the bot is connected.
            statusUpdateTimer = new Timer(RefreshBotActivity, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

            if (!await Connect())
            {
                Logger.Fatal("Aborting startup due to a fatal error when trying to connect to Discord.");
                return 2;
            }

            ServiceManager.Notify(ServiceState.Ready);

            // Loop here until application closes, handle any signals received
            while (await signalChannel.Reader.WaitToReadAsync())
            {
                while (signalChannel.Reader.TryRead(out PosixSignal signal))
                {
                    switch (signal)
                    {
                        case PosixSignal.SIGHUP:
                            // Tmds.Systemd.ServiceManager doesn't support the notify-reload service type so we have to send the reloading message manually.
                            // According to the documentation this shouldn't be the right way to calculate MONOTONIC_USEC, but it works for some reason.
                            byte[] data = System.Text.Encoding.UTF8.GetBytes($"RELOADING=1\nMONOTONIC_USEC={DateTimeOffset.UtcNow.ToUnixTimeMicroseconds()}\n");
                            UnixDomainSocketEndPoint ep = new(systemdSocket);
                            using (Socket cl = new(AddressFamily.Unix, SocketType.Dgram, ProtocolType.Unspecified))
                            {
                                await cl.ConnectAsync(ep);
                                cl.Send(data);
                            }

                            Reload();
                            ServiceManager.Notify(ServiceState.Ready);
                            break;
                        case PosixSignal.SIGTERM:
                            Logger.Log("Shutting down...");
                            ServiceManager.Notify(ServiceState.Stopping);
                            await client.DisconnectAsync();
                            client.Dispose();
                            return 0;
                        case PosixSignal.SIGINT:
                            Logger.Warn("Received interrupt signal, shutting down...");
                            ServiceManager.Notify(ServiceState.Stopping);
                            await client.DisconnectAsync();
                            client.Dispose();
                            return 0;
                        default:
                            break;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Logger.Fatal("Fatal error.", e);
            return 3;
        }

        return 0;
    }

    public static string GetVersion()
    {
        Version version = Assembly.GetEntryAssembly()?.GetName().Version;
        return version?.Major + "."
             + version?.Minor + "."
             + version?.Build
             + (version?.Revision == 0 ? "" : "-" + (char)(64 + version?.Revision ?? 0))
             + " (" + ThisAssembly.Git.Commit + ")";
    }

    public static bool Reload()
    {
        try
        {
            Config.LoadConfig();
        }
        catch (Exception e)
        {
            Logger.Fatal("Unable to read the config file: \"" + Config.ConfigPath + "\"", e);
            return false;
        }

        // Check if token is unset
        if (Config.token is "<add-token-here>" or "")
        {
            Logger.Fatal("You need to set your bot token in the config and start the bot again.");
            return false;
        }

        // Database connection and setup
        try
        {
            Logger.Log("Connecting to database. (" + Config.hostName + ":" + Config.port + ")");
            Database.SetConnectionString(Config.hostName, Config.port, Config.database, Config.username, Config.password);
            Database.SetupTables();
        }
        catch (Exception e)
        {
            Logger.Fatal("Could not set up database tables, please confirm connection settings, status of the server and permissions of MySQL user.", e);
            return false;
        }
        return true;
    }

    private static async Task<bool> Connect()
    {
        // Setting up client configuration
        DiscordConfiguration cfg = new DiscordConfiguration
        {
            Token = Config.token,
            TokenType = TokenType.Bot,
            MinimumLogLevel = LogLevel.Debug,
            AutoReconnect = true,
            Intents = DiscordIntents.AllUnprivileged | DiscordIntents.GuildMembers
        };

        client = new DiscordClient(cfg);

        Logger.Log("Hooking events...");
        client.Ready += EventHandler.OnReady;
        client.GuildAvailable += EventHandler.OnGuildAvailable;
        client.ClientErrored += EventHandler.OnClientError;
        client.GuildMemberAdded += EventHandler.OnGuildMemberAdded;
        client.GuildMemberRemoved += EventHandler.OnGuildMemberRemoved;

        Logger.Log("Connecting to Discord...");
        EventHandler.hasLoggedGuilds = false;

        try
        {
            await client.ConnectAsync();
        }
        catch (Exception e)
        {
            Logger.Fatal("Error occured while connecting to Discord.", e);
            return false;
        }

        return true;
    }

    internal static void RefreshBotActivity(object state = null)
    {
        try
        {
            if (!Enum.TryParse(Config.presenceType, true, out ActivityType activityType))
            {
                Logger.Log("Presence type '" + Config.presenceType + "' invalid, using 'Playing' instead.");
                activityType = ActivityType.Playing;
            }

            client.UpdateStatusAsync(new DiscordActivity(Config.presenceText, activityType), UserStatus.Online);
        }
        finally
        {
            statusUpdateTimer.Change(TimeSpan.FromMinutes(1), Timeout.InfiniteTimeSpan);
        }
    }
}
