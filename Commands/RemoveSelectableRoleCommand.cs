using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using RoleBoi;

namespace RoleBoi.Commands;

public class RemoveSelectableRoleCommand : ApplicationCommandModule
{
  [SlashRequireGuild]
  [SlashCommand("removeselectablerole", "Remove a role from the selectable list.")]
  public async Task OnExecute(InteractionContext command, [Option("role", "The role you want to remove.")] DiscordRole role)
  {
    if (Database.GetSelectableRoles().All(r => r != role.Id))
    {
      await command.CreateResponseAsync(new DiscordEmbedBuilder
      {
        Color = DiscordColor.Red,
        Description = "That role is not selectable."
      }, true);
      return;
    }

    if (!Database.TryRemoveSelectableRole(role.Id))
    {
      await command.CreateResponseAsync(new DiscordEmbedBuilder
      {
        Color = DiscordColor.Red,
        Description = "Failed to remove selectable role."
      }, true);
      return;
    }

    await command.CreateResponseAsync(new DiscordEmbedBuilder
    {
      Color = DiscordColor.Green,
      Description = "Selectable role removed."
    }, true);
  }
}