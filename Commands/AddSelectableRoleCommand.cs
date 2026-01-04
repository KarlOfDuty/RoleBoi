using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;

namespace RoleBoi.Commands;

public class AddSelectableRoleCommand
{
  [RequireGuild]
  [Command("addselectablerole")]
  [Description("Allow users to give themselves this role using the role selector.")]
  public async Task OnExecute(SlashCommandContext command, [Parameter("role")] [Description("The role you want to add.")] DiscordRole role)
  {
    if (Database.GetSelectableRoles().Any(r => r == role.Id))
    {
      await command.RespondAsync(new DiscordEmbedBuilder
      {
        Color = DiscordColor.Red,
        Description = "That role is already selectable."
      }, true);
      return;
    }

    if (!Database.TryAddSelectableRole(role.Id))
    {
      await command.RespondAsync(new DiscordEmbedBuilder
      {
        Color = DiscordColor.Red,
        Description = "Failed to add selectable role."
      }, true);
      return;
    }

    Logger.Log($"{command.Member.Username} ({command.Member.Id}) added the '{role.Name}' selectable role.");
    await command.RespondAsync(new DiscordEmbedBuilder
    {
      Color = DiscordColor.Green,
      Description = "Selectable role added."
    }, true);
  }
}