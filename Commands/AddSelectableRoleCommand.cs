using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using RoleBoi;

namespace RoleBoi.Commands;

public class AddSelectableRoleCommand : ApplicationCommandModule
{
  [SlashRequireGuild]
  [SlashCommand("addselectablerole", "Allow users to give themselves this role using the role selector.")]
  public async Task OnExecute(InteractionContext command, [Option("role", "The role you want to add.")] DiscordRole role)
  {
    if (Database.GetSelectableRoles().Any(r => r == role.Id))
    {
      await command.CreateResponseAsync(new DiscordEmbedBuilder
      {
        Color = DiscordColor.Red,
        Description = "That role is already selectable."
      }, true);
      return;
    }

    if (!Database.TryAddSelectableRole(role.Id))
    {
      await command.CreateResponseAsync(new DiscordEmbedBuilder
      {
        Color = DiscordColor.Red,
        Description = "Failed to add selectable role."
      }, true);
      return;
    }

    await command.CreateResponseAsync(new DiscordEmbedBuilder
    {
      Color = DiscordColor.Green,
      Description = "Selectable role added."
    }, true);
  }
}