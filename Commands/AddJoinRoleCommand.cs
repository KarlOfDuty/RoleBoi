using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace RoleBoi.Commands;

public class AddJoinRoleCommand : ApplicationCommandModule
{
  [SlashRequireGuild]
  [SlashCommand("addjoinrole", "Add a role for everyone to receive on join.")]
  public async Task OnExecute(InteractionContext command, [Option("role", "The role you want to add.")] DiscordRole role)
  {
    if (Database.GetJoinRoles().Any(r => r == role.Id))
    {
      await command.CreateResponseAsync(new DiscordEmbedBuilder
      {
        Color = DiscordColor.Red,
        Description = "That role is already configured as a join role."
      }, true);
      return;
    }

    if (!Database.TryAddJoinRole(role.Id))
    {
      await command.CreateResponseAsync(new DiscordEmbedBuilder
      {
        Color = DiscordColor.Red,
        Description = "Failed to add join role."
      }, true);
      return;
    }

    await command.CreateResponseAsync(new DiscordEmbedBuilder
    {
      Color = DiscordColor.Green,
      Description = "Join role added."
    }, true);
  }
}