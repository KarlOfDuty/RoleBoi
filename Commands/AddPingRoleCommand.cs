using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace RoleBoi.Commands;

public class AddPingRoleCommand : ApplicationCommandModule
{
  [SlashRequireGuild]
  [SlashCommand("addpingrole", "Allow a role to be mentioned via /ping.")]
  public async Task OnExecute(InteractionContext command, [Option("role", "The role you want to add.")] DiscordRole role)
  {
    if (Database.GetPingableRoles().Any(r => r == role.Id))
    {
      await command.CreateResponseAsync(new DiscordEmbedBuilder
      {
        Color = DiscordColor.Red,
        Description = "That role is already pingable."
      }, true);
      return;
    }

    if (!Database.TryAddPingableRole(role.Id))
    {
      await command.CreateResponseAsync(new DiscordEmbedBuilder
      {
        Color = DiscordColor.Red,
        Description = "Failed to add pingable role."
      }, true);
      return;
    }

    await command.CreateResponseAsync(new DiscordEmbedBuilder
    {
      Color = DiscordColor.Green,
      Description = "Pingable role added."
    }, true);
  }
}