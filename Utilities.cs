using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

namespace RoleBoi;

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
  // CLOCK_MONOTONIC is the specific clock we want to read in the clock_gettime function and has the ID 1.
  private const int CLOCK_MONOTONIC = 1;

  [StructLayout(LayoutKind.Sequential)]
  private struct Timespec
  {
    public long tv_sec;
    public long tv_nsec;
  }

  [SupportedOSPlatform("linux")]
  [DllImport("libc", EntryPoint = "clock_gettime", SetLastError = true)]
  private static extern int clock_gettime(int clk_id, out Timespec tp);

  public static long GetMonotonicUsec()
  {
    if (!OperatingSystem.IsLinux())
    {
      return -1;
    }

    try
    {
      if (clock_gettime(CLOCK_MONOTONIC, out Timespec ts) == 0)
      {
        checked
        {
          return ts.tv_sec * 1000000 + ts.tv_nsec / 1000;
        }
      }
    }
    catch { /* ignored */ }

    return Environment.TickCount64 * 1000;
  }

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