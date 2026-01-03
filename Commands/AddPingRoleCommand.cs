using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;

namespace RoleBoi.Commands;

public class AddPingRoleCommand
{
  [RequireGuild]
  [Command("addpingrole")]
  [Description("Allow a role to be mentioned via /ping.")]
  public async Task OnExecute(SlashCommandContext command, [Parameter("role")] [Description("The role you want to add.")] DiscordRole role)
  {
    if (Database.GetPingableRoles().Any(r => r == role.Id))
    {
      await command.RespondAsync(new DiscordEmbedBuilder
      {
        Color = DiscordColor.Red,
        Description = "That role is already pingable."
      }, true);
      return;
    }

    if (!Database.TryAddPingableRole(role.Id))
    {
      await command.RespondAsync(new DiscordEmbedBuilder
      {
        Color = DiscordColor.Red,
        Description = "Failed to add pingable role."
      }, true);
      return;
    }

    Logger.Log($"{command.Member.Username} ({command.Member.Id}) added the '{role.Name}' pingable role.");
    await command.RespondAsync(new DiscordEmbedBuilder
    {
      Color = DiscordColor.Green,
      Description = "Pingable role added."
    }, true);
  }
}