using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using RoleBoi;

namespace RoleBoi.Commands;

public class RemovePingRoleCommand : ApplicationCommandModule
{
  [SlashRequireGuild]
  [SlashCommand("removepingrole", "Make a role no longer allowed to mention using /ping.")]
  public async Task OnExecute(InteractionContext command, [Option("role", "The role you want to remove.")] DiscordRole role)
  {
    if (Database.GetPingableRoles().All(r => r != role.Id))
    {
      await command.CreateResponseAsync(new DiscordEmbedBuilder
      {
        Color = DiscordColor.Red,
        Description = "That role is not pingable."
      }, true);
      return;
    }

    if (!Database.TryRemovePingableRole(role.Id))
    {
      await command.CreateResponseAsync(new DiscordEmbedBuilder
      {
        Color = DiscordColor.Red,
        Description = "Failed to remove pingable role."
      }, true);
      return;
    }

    await command.CreateResponseAsync(new DiscordEmbedBuilder
    {
      Color = DiscordColor.Green,
      Description = "Pingable role removed."
    }, true);
  }
}