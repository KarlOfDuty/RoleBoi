using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;

namespace MuteBoi
{
  static class EventHandler
  {
    internal static bool hasLoggedGuilds = false;

    public static Task OnReady(DiscordClient client, ReadyEventArgs e)
    {
      Logger.Log("Client is ready to process events.");

      // Checking activity type
      if (!Enum.TryParse(Config.presenceType, true, out ActivityType activityType))
      {
        Console.WriteLine("Presence type '" + Config.presenceType + "' invalid, using 'Playing' instead.");
        activityType = ActivityType.Playing;
      }

      client.UpdateStatusAsync(new DiscordActivity(Config.presenceText, activityType), UserStatus.Online);
      hasLoggedGuilds = true;
      return Task.CompletedTask;
    }

    public static async Task OnGuildAvailable(DiscordClient _, GuildCreateEventArgs e)
    {
      if (hasLoggedGuilds)
      {
        return;
      }

      Logger.Log("Found Discord server: " + e.Guild.Name + " (" + e.Guild.Id + ")");

      if (MuteBoi.commandLineArgs.serversToLeave.Contains(e.Guild.Id))
      {
        Logger.Warn("LEAVING DISCORD SERVER AS REQUESTED: " + e.Guild.Name + " (" + e.Guild.Id + ")");
        await e.Guild.LeaveAsync();
        return;
      }

      IReadOnlyDictionary<ulong, DiscordRole> roles = e.Guild.Roles;
      foreach ((ulong roleID, DiscordRole role) in roles)
      {
        Logger.Log(role.Name.PadRight(40, '.') + roleID);
      }
    }

    public static Task OnClientError(DiscordClient _, ClientErrorEventArgs e)
    {
      Logger.Error("Exception occured:\n" + e.Exception);
      return Task.CompletedTask;
    }

    public static Task OnGuildMemberRemoved(DiscordClient _, GuildMemberRemoveEventArgs e)
    {
      foreach (DiscordRole role in e.Member.Roles)
      {
        if (Config.trackedRoles.Contains(role.Id))
        {
          Logger.Log(e.Member.DisplayName + " (" + e.Member.Id + ") left the server with tracked role '" + role.Name + "'.");
          Database.TryAddRole(e.Member.Id, role.Id);
        }
      }
      return Task.CompletedTask;
    }

    public static async Task OnGuildMemberAdded(DiscordClient _, GuildMemberAddEventArgs e)
    {
      if (!Database.TryGetRoles(e.Member.Id, out List<Database.SavedRole> savedRoles)) return;

      foreach (Database.SavedRole savedRole in savedRoles)
      {
        try
        {

          DiscordRole role = e.Guild.GetRole(savedRole.roleID);
          Logger.Log(e.Member.DisplayName + " (" + e.Member.Id + ") were given back the role '" + role.Name + "' on rejoin. ");
          await e.Member.GrantRoleAsync(role);
        }
        catch (NotFoundException) {}
        catch (UnauthorizedException) {}
      }

      Database.TryRemoveRoles(e.Member.Id);
    }
  }
}
