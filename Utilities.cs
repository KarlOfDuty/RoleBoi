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
  // Use CLOCK_MONOTONIC for MONOTONIC_USEC as expected by systemd notify-reload.
  // See: sd_notify(3) documentation.
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

  [SupportedOSPlatform("linux")]
  public static long GetMonotonicUsec()
  {
    try
    {
      if (clock_gettime(CLOCK_MONOTONIC, out Timespec ts) == 0)
      {
        checked
        {
          return ts.tv_sec * 1_000_000 + ts.tv_nsec / 1000;
        }
      }
    }
    catch
    {
      // ignore and use fallback below
    }

    // Final fallback in case of any error
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