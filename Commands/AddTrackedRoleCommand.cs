using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace RoleBoi.Commands;

public class AddTrackedRoleCommand : ApplicationCommandModule
{
  [SlashRequireGuild]
  [SlashCommand("addtrackedrole", "If users with this role leave the server they will get it back if they rejoin.")]
  public async Task OnExecute(InteractionContext command, [Option("role", "The role you want to add.")] DiscordRole role)
  {
    if (Database.GetTrackedRoles().Any(r => r == role.Id))
    {
      await command.CreateResponseAsync(new DiscordEmbedBuilder
      {
        Color = DiscordColor.Red,
        Description = "That role is already tracked."
      }, true);
      return;
    }

    if (!Database.TryAddTrackedRole(role.Id))
    {
      await command.CreateResponseAsync(new DiscordEmbedBuilder
      {
        Color = DiscordColor.Red,
        Description = "Failed to add tracked role."
      }, true);
      return;
    }

    await command.CreateResponseAsync(new DiscordEmbedBuilder
    {
      Color = DiscordColor.Green,
      Description = "Tracked role added."
    }, true);
  }
}