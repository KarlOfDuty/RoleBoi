using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using MuteBoi.Properties;
using YamlDotNet.Serialization;

namespace MuteBoi
{
  internal static class Config
  {
    internal const string APPLICATION_NAME = "MuteBoi";

    internal static string token = "";
    internal static string logLevel = "Info";
    internal static ulong[] trackedRoles = [];
    internal static string presenceType = "Playing";
    internal static string presenceText = "";

    internal static string hostName = "127.0.0.1";
    internal static int    port     = 3306;
    internal static string database = "muteboi";
    internal static string username = "";
    internal static string password = "";

    public static void LoadConfig()
    {
      // Writes default config to file if it does not already exist
      if (!File.Exists("./config.yml"))
      {
        File.WriteAllText("./config.yml", Encoding.UTF8.GetString(Resources.default_config));
      }

      // Reads config contents into FileStream
      FileStream stream = File.OpenRead("./config.yml");

      // Converts the FileStream into a YAML object
      IDeserializer deserializer = new DeserializerBuilder().Build();
      object yamlObject = deserializer.Deserialize(new StreamReader(stream));

      // Converts the YAML object into a JSON object as the YAML ones do not support traversal or selection of nodes by name
      ISerializer serializer = new SerializerBuilder().JsonCompatible().Build();
      JObject json = JObject.Parse(serializer.Serialize(yamlObject));

      token = json.SelectToken("bot.token").Value<string>() ?? "";
      logLevel = json.SelectToken("bot.console-log-level").Value<string>() ?? "";
      trackedRoles = json.SelectToken("bot.tracked-roles").Value<JArray>().Values<ulong>().ToArray();
      presenceType = json.SelectToken("bot.presence-type")?.Value<string>() ?? "Playing";
      presenceText = json.SelectToken("bot.presence-text")?.Value<string>() ?? "";

      // Reads database info
      hostName = json.SelectToken("database.address").Value<string>() ?? "";
      port = json.SelectToken("database.port").Value<int>();
      database = json.SelectToken("database.name").Value<string>() ?? "";
      username = json.SelectToken("database.user").Value<string>() ?? "";
      password = json.SelectToken("database.password").Value<string>() ?? "";
    }
  }
}
