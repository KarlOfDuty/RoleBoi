using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using YamlDotNet.Serialization;

namespace RoleBoi;

internal static class Config
{
  internal static string token = "";
  internal static ulong[] trackedRoles = [];
  internal static ulong[] everyoneRoles = [];
  internal static string presenceType = "Playing";
  internal static string presenceText = "";

  internal static string databaseFile = "./roleboi.db";

  public static string ConfigPath { get; private set; } = "./config.yml";
  public static string LogPath { get; private set; } = "";

  public static bool Initialized { get; private set; } = false;

  public static void LoadConfig()
  {
    if (!string.IsNullOrEmpty(RoleBoi.commandLineArgs.configPath))
    {
      ConfigPath = RoleBoi.commandLineArgs.configPath;
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
    if (!string.IsNullOrEmpty(RoleBoi.commandLineArgs.logFilePath))
    {
      LogPath = RoleBoi.commandLineArgs.logFilePath;
    }

    string stringLogLevel = json.SelectToken("bot.console-log-level")?.Value<string>() ?? "";
    if (!Enum.TryParse(stringLogLevel, true, out LogLevel logLevel))
    {
      logLevel = LogLevel.Information;
      Logger.Warn("Log level '" + stringLogLevel + "' is invalid, using 'Information' instead.");
    }
    Logger.SetLogLevel(logLevel);

    token = json.SelectToken("bot.token")?.Value<string>() ?? "";
    trackedRoles = json.SelectToken("bot.tracked-roles")?.Value<JArray>().Values<ulong>().ToArray() ?? []; //TODO: Read these from database instead
    everyoneRoles = json.SelectToken("bot.everyone-roles")?.Value<JArray>().Values<ulong>().ToArray() ?? []; //TODO: Read these from database instead
    presenceType = json.SelectToken("bot.presence-type")?.Value<string>() ?? "Playing";
    presenceText = json.SelectToken("bot.presence-text")?.Value<string>() ?? "";
    databaseFile = json.SelectToken("bot.database-file")?.Value<string>() ?? "./roleboi.db";

    Initialized = true;
  }
}
