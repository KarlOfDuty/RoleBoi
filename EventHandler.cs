using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands.EventArgs;

namespace RoleBoi
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

      if (RoleBoi.commandLineArgs.serversToLeave.Contains(e.Guild.Id))
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
      List<ulong> trackedRoles = Database.GetTrackedRoles();
      foreach (DiscordRole role in e.Member.Roles)
      {
        if (trackedRoles.Contains(role.Id))
        {
          Logger.Log(e.Member.DisplayName + " (" + e.Member.Id + ") left the server with tracked role '" + role.Name + "'.");
          Database.TryAddUserRole(e.Member.Id, role.Id);
        }
      }
      return Task.CompletedTask;
    }

    public static async Task OnGuildMemberAdded(DiscordClient _, GuildMemberAddEventArgs e)
    {
      List<ulong> joinRoles = Database.GetJoinRoles();
      foreach (ulong roleID in joinRoles)
      {
        try
        {
          DiscordRole role = e.Guild.GetRole(roleID);
          await e.Member.GrantRoleAsync(role);
          Logger.Log(e.Member.DisplayName + " (" + e.Member.Id + ") was given the '" + role.Name + "' role. ");
        }
        catch (NotFoundException) {}
        catch (UnauthorizedException) {}
      }

      if (!Database.TryGetUserRoles(e.Member.Id, out List<Database.SavedRole> savedRoles)) return;

      foreach (Database.SavedRole savedRole in savedRoles)
      {
        try
        {
          DiscordRole role = e.Guild.GetRole(savedRole.roleID);
          await e.Member.GrantRoleAsync(role);
          Logger.Log(e.Member.DisplayName + " (" + e.Member.Id + ") was given back the '" + role.Name + "' role on rejoin. ");
        }
        catch (NotFoundException) {}
        catch (UnauthorizedException) {}
      }

      Database.TryRemoveUserRoles(e.Member.Id);
    }


    internal static Task OnCommandError(SlashCommandsExtension commandSystem, SlashCommandErrorEventArgs e)
    {
      switch (e.Exception)
      {
        case SlashExecutionChecksFailedException checksFailedException:
        {
          foreach (SlashCheckBaseAttribute attr in checksFailedException.FailedChecks)
          {
            DiscordEmbed error = new DiscordEmbedBuilder
            {
              Color = DiscordColor.Red,
              Description = ParseFailedCheck(attr)
            };
            e.Context.CreateResponseAsync(error);
          }
          return Task.CompletedTask;
        }
        default:
        {
          Logger.Error("Exception occured: " + e.Exception.GetType(), e.Exception);
          if (e.Exception is UnauthorizedException ex)
          {
            Logger.Error(ex.WebResponse.Response);
          }

          DiscordEmbed error = new DiscordEmbedBuilder
          {
            Color = DiscordColor.Red,
            Description = "Internal error occured, please report this to the developer."
          };
          e.Context.CreateResponseAsync(error);
          return Task.CompletedTask;
        }
      }
    }

    internal static async Task OnComponentInteractionCreated(DiscordClient client, ComponentInteractionCreateEventArgs e)
    {
      try
      {
        switch (e.Interaction.Data.ComponentType)
        {
          case ComponentType.StringSelect:
            if (!e.Interaction.Data.CustomId.StartsWith("rolemanager_togglerole"))
            {
              return;
            }

            if (e.Interaction.Data.Values.Length == 0)
            {
              await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                new DiscordInteractionResponseBuilder().WithContent(e.Message.Content).AddComponents(e.Message.Components));
            }

            foreach (string stringID in e.Interaction.Data.Values)
            {
              if (!ulong.TryParse(stringID, out ulong roleID) || roleID == 0) continue;

              DiscordMember member = await e.Guild.GetMemberAsync(e.User.Id);
              if (!e.Guild.Roles.ContainsKey(roleID) || member == null) continue;

              if (member.Roles.Any(role => role.Id == roleID))
              {
                await member.RevokeRoleAsync(e.Guild.Roles[roleID]);
                await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                  new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder
                  {
                    Color = DiscordColor.Green,
                    Description = "Revoked role " + e.Guild.Roles[roleID].Mention + "!"
                  }).AsEphemeral());
              }
              else
              {
                await member.GrantRoleAsync(e.Guild.Roles[roleID]);
                await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                  new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder
                  {
                    Color = DiscordColor.Green,
                    Description = "Granted role " + e.Guild.Roles[roleID].Mention + "!"
                  }).AsEphemeral());
              }
            }
            break;

          case ComponentType.ActionRow:
          case ComponentType.Button:
          case ComponentType.FormInput:
            return;
        }
      }
      catch (UnauthorizedException)
      {
        await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
          new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder
          {
            Color = DiscordColor.Red,
            Description = "The bot doesn't have the required permissions to do that!"
          }).AsEphemeral());
      }
      catch (Exception ex)
      {
        Logger.Error("Exception occured: " + ex.GetType(), ex);
        await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
          new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder
          {
            Color = DiscordColor.Red,
            Description = "Internal interaction error occured, please report this to the developer."
          }).AsEphemeral());
      }
    }

    private static string ParseFailedCheck(SlashCheckBaseAttribute attr)
    {
      return attr switch
      {
        SlashRequireDirectMessageAttribute _ => "This command can only be used in direct messages!",
        SlashRequireOwnerAttribute _ => "Only the server owner can use that command!",
        SlashRequirePermissionsAttribute _ => "You don't have permission to do that!",
        SlashRequireBotPermissionsAttribute _ => "The bot doesn't have the required permissions to do that!",
        SlashRequireUserPermissionsAttribute _ => "You don't have permission to do that!",
        SlashRequireGuildAttribute _ => "This command has to be used in a Discord server!",
        _ => "Unknown Discord API error occured, please try again later."
      };
    }
  }
}
