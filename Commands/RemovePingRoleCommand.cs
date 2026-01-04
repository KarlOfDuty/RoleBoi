using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;

namespace RoleBoi.Commands;

public class RemovePingRoleCommand
{
  [RequireGuild]
  [Command("removepingrole")]
  [Description("Make a role no longer allowed to mention using /ping.")]
  public async Task OnExecute(SlashCommandContext command, [Parameter("role")] [Description("The role you want to remove.")] DiscordRole role)
  {
    if (Database.GetPingableRoles().All(r => r != role.Id))
    {
      await command.RespondAsync(new DiscordEmbedBuilder
      {
        Color = DiscordColor.Red,
        Description = "That role is not pingable."
      }, true);
      return;
    }

    if (!Database.TryRemovePingableRole(role.Id))
    {
      await command.RespondAsync(new DiscordEmbedBuilder
      {
        Color = DiscordColor.Red,
        Description = "Failed to remove pingable role."
      }, true);
      return;
    }

    Logger.Log($"{command.Member.Username} ({command.Member.Id}) removed the '{role.Name}' pingable role.");
    await command.RespondAsync(new DiscordEmbedBuilder
    {
      Color = DiscordColor.Green,
      Description = "Pingable role removed."
    }, true);
  }
}