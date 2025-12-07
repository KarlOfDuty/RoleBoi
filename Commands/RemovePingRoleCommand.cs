using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using RoleBoi;

namespace RoleManager.Commands;

public class RemovePingRoleCommand : ApplicationCommandModule
{
  [SlashRequireGuild]
  [SlashCommand("removerole", "Removes a Discord role from the bot")]
  public async Task OnExecute(InteractionContext command, [Option("Role", "The role you want to remove.")] DiscordRole role)
  {
    // TODO: Update for RoleBoi

    if (Roles.savedRoles.All(savedRole => savedRole != role.Id))
    {
      await command.CreateResponseAsync(new DiscordEmbedBuilder
      {
        Color = DiscordColor.Red,
        Description = "That role is already disabled."
      }, true);
      return;
    }

    if (!command.Guild.Roles.ContainsKey(role.Id))
    {
      await command.CreateResponseAsync(new DiscordEmbedBuilder
      {
        Color = DiscordColor.Red,
        Description = "That role doesn't exist."
      }, true);
      return;
    }

    Roles.RemoveRole(role.Id);

    await command.CreateResponseAsync(new DiscordEmbedBuilder
    {
      Color = DiscordColor.Green,
      Description = "Role removed."
    }, true);
  }
}