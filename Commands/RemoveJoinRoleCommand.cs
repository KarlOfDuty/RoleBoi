using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using RoleBoi;

namespace RoleBoi.Commands;

public class RemoveJoinRoleCommand : ApplicationCommandModule
{
  [SlashRequireGuild]
  [SlashCommand("removejoinrole", "Remove a role from being assigned to everyone who joins.")]
  public async Task OnExecute(InteractionContext command, [Option("role", "The role you want to remove.")] DiscordRole role)
  {
    if (Database.GetJoinRoles().All(r => r != role.Id))
    {
      await command.CreateResponseAsync(new DiscordEmbedBuilder
      {
        Color = DiscordColor.Red,
        Description = "That role is not configured as a join role."
      }, true);
      return;
    }

    if (!Database.TryRemoveJoinRole(role.Id))
    {
      await command.CreateResponseAsync(new DiscordEmbedBuilder
      {
        Color = DiscordColor.Red,
        Description = "Failed to remove join role."
      }, true);
      return;
    }

    Logger.Log($"{command.Member.Username} ({command.Member.Id}) removed the '{role.Name}' join role.");
    await command.CreateResponseAsync(new DiscordEmbedBuilder
    {
      Color = DiscordColor.Green,
      Description = "Join role removed."
    }, true);
  }
}