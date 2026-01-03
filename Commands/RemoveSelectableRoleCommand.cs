using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;

namespace RoleBoi.Commands;

public class RemoveSelectableRoleCommand
{
  [RequireGuild]
  [Command("removeselectablerole")]
  [Description("Remove a role from the selectable list.")]
  public async Task OnExecute(SlashCommandContext command, [Parameter("role")] [Description("The role you want to remove.")] DiscordRole role)
  {
    if (Database.GetSelectableRoles().All(r => r != role.Id))
    {
      await command.RespondAsync(new DiscordEmbedBuilder
      {
        Color = DiscordColor.Red,
        Description = "That role is not selectable."
      }, true);
      return;
    }

    if (!Database.TryRemoveSelectableRole(role.Id))
    {
      await command.RespondAsync(new DiscordEmbedBuilder
      {
        Color = DiscordColor.Red,
        Description = "Failed to remove selectable role."
      }, true);
      return;
    }

    Logger.Log($"{command.Member.Username} ({command.Member.Id}) removed the '{role.Name}' selectable role.");
    await command.RespondAsync(new DiscordEmbedBuilder
    {
      Color = DiscordColor.Green,
      Description = "Selectable role removed."
    }, true);
  }
}