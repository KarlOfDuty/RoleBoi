using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MuteBoi;

public static class Extensions
{
  private static readonly DateTimeOffset UnixEpoch =
    new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

  public static long ToUnixTimeMicroseconds(this DateTimeOffset timestamp)
  {
    TimeSpan duration = timestamp - UnixEpoch;
    // There are 10 ticks per microsecond.
    return duration.Ticks / 10;
  }
}

public static class Utilities
{
  public static string ReadManifestData(string embeddedFileName)
  {
    Assembly assembly = Assembly.GetExecutingAssembly();
    string resourceName = assembly.GetManifestResourceNames().First(s => s.EndsWith(embeddedFileName,StringComparison.CurrentCultureIgnoreCase));

    using Stream stream = assembly.GetManifestResourceStream(resourceName);
    if (stream == null)
    {
      throw new InvalidOperationException("Could not load manifest resource stream.");
    }

    using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
    return reader.ReadToEnd();
  }
}