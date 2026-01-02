using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using RoleBoi;

namespace RoleBoi.Commands;

public class RemoveTrackedRoleCommand : ApplicationCommandModule
{
  [SlashRequireGuild]
  [SlashCommand("removetrackedrole", "Stop tracking members with this role rejoining.")]
  public async Task OnExecute(InteractionContext command, [Option("role", "The role you want to remove.")] DiscordRole role)
  {
    if (Database.GetTrackedRoles().All(r => r != role.Id))
    {
      await command.CreateResponseAsync(new DiscordEmbedBuilder
      {
        Color = DiscordColor.Red,
        Description = "That role is not tracked."
      }, true);
      return;
    }

    if (!Database.TryRemoveTrackedRole(role.Id))
    {
      await command.CreateResponseAsync(new DiscordEmbedBuilder
      {
        Color = DiscordColor.Red,
        Description = "Failed to remove tracked role."
      }, true);
      return;
    }

    Logger.Log($"{command.Member.Username} ({command.Member.Id}) removed the '{role.Name}' tracked role.");
    await command.CreateResponseAsync(new DiscordEmbedBuilder
    {
      Color = DiscordColor.Green,
      Description = "Tracked role removed."
    }, true);
  }
}