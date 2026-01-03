using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;

namespace RoleBoi.Commands;

public class RemoveJoinRoleCommand
{
  [RequireGuild]
  [Command("removejoinrole")]
  [Description("Remove a role from being assigned to everyone who joins.")]
  public async Task OnExecute(SlashCommandContext command, [Parameter("role")] [Description("The role you want to remove.")] DiscordRole role)
  {
    if (Database.GetJoinRoles().All(r => r != role.Id))
    {
      await command.RespondAsync(new DiscordEmbedBuilder
      {
        Color = DiscordColor.Red,
        Description = "That role is not configured as a join role."
      }, true);
      return;
    }

    if (!Database.TryRemoveJoinRole(role.Id))
    {
      await command.RespondAsync(new DiscordEmbedBuilder
      {
        Color = DiscordColor.Red,
        Description = "Failed to remove join role."
      }, true);
      return;
    }

    Logger.Log($"{command.Member.Username} ({command.Member.Id}) removed the '{role.Name}' join role.");
    await command.RespondAsync(new DiscordEmbedBuilder
    {
      Color = DiscordColor.Green,
      Description = "Join role removed."
    }, true);
  }
}