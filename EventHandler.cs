using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.EventArgs;
using DSharpPlus.Commands.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;

namespace RoleBoi
{
  static class EventHandler
  {
    internal static bool hasLoggedGuilds = false;

    public static Task OnReady(DiscordClient client, GuildDownloadCompletedEventArgs e)
    {
      Logger.Log("Connected to Discord.");

      RoleBoi.RefreshBotActivity();
      hasLoggedGuilds = true;
      return Task.CompletedTask;
    }

    public static async Task OnGuildAvailable(DiscordClient discordClient, GuildAvailableEventArgs e)
    {
      if (hasLoggedGuilds)
      {
        return;
      }

      Logger.Log("Found Discord server: " + e.Guild.Name + " (" + e.Guild.Id + ")");

      if (RoleBoi.commandLineArgs.ServersToLeave.Contains(e.Guild.Id))
      {
        Logger.Warn("LEAVING DISCORD SERVER AS REQUESTED: " + e.Guild.Name + " (" + e.Guild.Id + ")");
        await e.Guild.LeaveAsync();
        return;
      }

      IReadOnlyDictionary<ulong, DiscordRole> roles = e.Guild.Roles;

      foreach ((ulong roleID, DiscordRole role) in roles)
      {
        Logger.Debug(role.Name.PadRight(40, '.') + roleID);
      }
    }

    public static Task OnGuildMemberRemoved(DiscordClient _, GuildMemberRemovedEventArgs e)
    {
      List<ulong> trackedRoles = Database.GetTrackedRoles();
      foreach (DiscordRole role in e.Member.Roles)
      {
        if (trackedRoles.Contains(role.Id))
        {
          Logger.Log($"{e.Member.Username} ({e.Member.Id}) left the server with tracked role '{role.Name}'.");
          Database.TryAddUserRole(e.Member.Id, role.Id);
        }
      }
      return Task.CompletedTask;
    }

    public static async Task OnGuildMemberAdded(DiscordClient _, GuildMemberAddedEventArgs e)
    {
      List<ulong> joinRoles = Database.GetJoinRoles();
      foreach (ulong roleID in joinRoles)
      {
        try
        {
          DiscordRole role = await e.Guild.GetRoleAsync(roleID);
          await e.Member.GrantRoleAsync(role);
          Logger.Log($"{e.Member.Username} ({e.Member.Id}) was given the '{role.Name}' role on join.");
        }
        catch (Exception ex)
        {
          Logger.Error($"Error occurred when attempting to add join role {roleID} to member {e.Member.Username}", ex);
        }
      }

      if (!Database.TryGetUserRoles(e.Member.Id, out List<Database.SavedRole> savedRoles)) return;

      foreach (Database.SavedRole savedRole in savedRoles)
      {
        try
        {
          DiscordRole role = await e.Guild.GetRoleAsync(savedRole.roleID);
          await e.Member.GrantRoleAsync(role);
          Logger.Log($"{e.Member.Username} ({e.Member.Id}) was given back the '{role.Name}' role on rejoin.");
        }
        catch (Exception ex)
        {
          Logger.Error($"Error occurred when attempting to add tracked role {savedRole.roleID} to member {e.Member.Username}", ex);
        }
      }

      Database.TryRemoveUserRoles(e.Member.Id);
    }


    public static async Task OnCommandError(CommandsExtension commandSystem, CommandErroredEventArgs e)
    {
      try
      {
        switch (e.Exception)
        {
          case ChecksFailedException checksFailedException:
          {
            foreach (ContextCheckFailedData error in checksFailedException.Errors)
            {
              await e.Context.Channel.SendMessageAsync(new DiscordEmbedBuilder
              {
                Color = DiscordColor.Red,
                Description = error.ErrorMessage
              });
            }
            return;
          }

          case BadRequestException ex:
            Logger.Error("Command exception occured.", e.Exception);
            Logger.Error("JSON Message: " + ex.JsonMessage);
            return;

          default:
          {
            Logger.Error("Command exception occured.", e.Exception);
            await e.Context.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
              Color = DiscordColor.Red,
              Description = "Internal error occured, please report this to the developer."
            });
            return;
          }
        }
      }
      catch (Exception ex)
      {
        Logger.Error("An error occurred in command error handler.", ex);
        Logger.Error("Original exception:", e.Exception);
      }
    }

    internal static async Task OnComponentInteractionCreated(DiscordClient client, ComponentInteractionCreatedEventArgs e)
    {
      try
      {
        if (e.Interaction.Data.ComponentType != DiscordComponentType.StringSelect)
        {
          Logger.Warn("Unknown interaction type received! '" + e.Interaction.Data.ComponentType + "'");
          return;
        }

        if (!e.Interaction.Data.CustomId.StartsWith("roleboi_togglerole"))
        {
          return;
        }

        if (e.Interaction.Data.Values.Length == 0)
        {
          await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage);
        }

        foreach (string stringID in e.Interaction.Data.Values)
        {
          if (!ulong.TryParse(stringID, out ulong roleID) || roleID == 0) continue;

          DiscordMember member = await e.Guild.GetMemberAsync(e.User.Id);
          if (!e.Guild.Roles.ContainsKey(roleID) || member == null) continue;

          if (member.Roles.Any(role => role.Id == roleID))
          {
            await member.RevokeRoleAsync(e.Guild.Roles[roleID]);
            await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
              new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder
              {
                Color = DiscordColor.Green,
                Description = "Revoked role " + e.Guild.Roles[roleID].Mention + "!"
              }).AsEphemeral());
          }
          else
          {
            await member.GrantRoleAsync(e.Guild.Roles[roleID]);
            await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
              new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder
              {
                Color = DiscordColor.Green,
                Description = "Granted role " + e.Guild.Roles[roleID].Mention + "!"
              }).AsEphemeral());
          }
        }
      }
      catch (UnauthorizedException)
      {
        await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
          new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder
          {
            Color = DiscordColor.Red,
            Description = "The bot doesn't have the required permissions to do that!"
          }).AsEphemeral());
      }
      catch (Exception ex)
      {
        Logger.Error("Exception occured: " + ex.GetType(), ex);
        await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
          new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder
          {
            Color = DiscordColor.Red,
            Description = "Internal interaction error occured, please report this to the developer."
          }).AsEphemeral());
      }
    }
  }

  internal class ErrorHandler : IClientErrorHandler
  {
    public ValueTask HandleEventHandlerError(string name,
      Exception exception,
      Delegate invokedDelegate,
      object sender,
      object args)
    {
      Logger.Error("Client exception occured:\n" + exception);
      if (exception is BadRequestException ex)
      {
        Logger.Error("JSON Message: " + ex.JsonMessage);
      }

      return ValueTask.FromException(exception);
    }

    public ValueTask HandleGatewayError(Exception exception)
    {
      Logger.Error("A gateway error occured:\n" + exception);
      return ValueTask.FromException(exception);
    }
  }
}
