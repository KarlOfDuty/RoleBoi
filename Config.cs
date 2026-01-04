using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using YamlDotNet.Serialization;

namespace RoleBoi;

internal static class Config
{
  public static string Token { get; private set; } = "";
  public static string PresenceType { get; private set; } = "Playing";
  public static string PresenceText { get; private set; } = "";
  public static string DatabaseFile { get; private set; } = "./roleboi.db";
  public static string ConfigPath { get; private set; } = "./config.yml";
  public static string LogPath { get; private set; } = "";

  public static bool Initialized { get; private set; } = false;

  public static void LoadConfig()
  {
    if (!string.IsNullOrEmpty(RoleBoi.commandLineArgs.ConfigPath))
    {
      ConfigPath = RoleBoi.commandLineArgs.ConfigPath;
    }

    Logger.Log("Loading config \"" + Path.GetFullPath(ConfigPath) + "\"");

    // Writes default config to file if it does not already exist
    if (!File.Exists(ConfigPath))
    {
      File.WriteAllText(ConfigPath, Utilities.ReadManifestData("default_config.yml"));
    }

    // Reads config contents into FileStream
    FileStream stream = File.OpenRead(ConfigPath);

    // Converts the FileStream into a YAML object
    IDeserializer deserializer = new DeserializerBuilder().Build();
    object yamlObject = deserializer.Deserialize(new StreamReader(stream));

    // Converts the YAML object into a JSON object as the YAML ones do not support traversal or selection of nodes by name
    ISerializer serializer = new SerializerBuilder().JsonCompatible().Build();
    JObject json = JObject.Parse(serializer.Serialize(yamlObject));

    LogPath = json.SelectToken("bot.log-file")?.Value<string>() ?? "";
    if (!string.IsNullOrEmpty(RoleBoi.commandLineArgs.LogFilePath))
    {
      LogPath = RoleBoi.commandLineArgs.LogFilePath;
    }

    DatabaseFile = json.SelectToken("bot.database-file")?.Value<string>() ?? "./roleboi.db";
    if (!string.IsNullOrEmpty(RoleBoi.commandLineArgs.DatabasePath))
    {
      DatabaseFile = RoleBoi.commandLineArgs.DatabasePath;
    }

    string stringLogLevel = json.SelectToken("bot.console-log-level")?.Value<string>() ?? "";
    if (!Enum.TryParse(stringLogLevel, true, out LogLevel logLevel))
    {
      logLevel = LogLevel.Information;
      Logger.Warn("Log level '" + stringLogLevel + "' is invalid, using 'Information' instead.");
    }
    Logger.SetLogLevel(logLevel);

    Token = json.SelectToken("bot.token")?.Value<string>() ?? "";
    PresenceType = json.SelectToken("bot.presence-type")?.Value<string>() ?? "Playing";
    PresenceText = json.SelectToken("bot.presence-text")?.Value<string>() ?? "";

    Initialized = true;
  }
}
