using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;

namespace RoleBoi.Commands;

public class RemoveTrackedRoleCommand
{
  [RequireGuild]
  [Command("removetrackedrole")]
  [Description("Stop tracking members with this role rejoining.")]
  public async Task OnExecute(SlashCommandContext command, [Parameter("role")] [Description("The role you want to remove.")] DiscordRole role)
  {
    if (Database.GetTrackedRoles().All(r => r != role.Id))
    {
      await command.RespondAsync(new DiscordEmbedBuilder
      {
        Color = DiscordColor.Red,
        Description = "That role is not tracked."
      }, true);
      return;
    }

    if (!Database.TryRemoveTrackedRole(role.Id))
    {
      await command.RespondAsync(new DiscordEmbedBuilder
      {
        Color = DiscordColor.Red,
        Description = "Failed to remove tracked role."
      }, true);
      return;
    }

    Logger.Log($"{command.Member.Username} ({command.Member.Id}) removed the '{role.Name}' tracked role.");
    await command.RespondAsync(new DiscordEmbedBuilder
    {
      Color = DiscordColor.Green,
      Description = "Tracked role removed."
    }, true);
  }
}