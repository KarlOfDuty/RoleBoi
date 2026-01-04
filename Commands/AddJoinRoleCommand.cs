using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;

namespace RoleBoi.Commands;

public class AddJoinRoleCommand
{
  [RequireGuild]
  [Command("addjoinrole")]
  [Description("Add a role for everyone to receive on join.")]
  public async Task OnExecute(SlashCommandContext command, [Parameter("role")] [Description("The role you want to add.")] DiscordRole role)
  {
    if (Database.GetJoinRoles().Any(r => r == role.Id))
    {
      await command.RespondAsync(new DiscordEmbedBuilder
      {
        Color = DiscordColor.Red,
        Description = "That role is already configured as a join role."
      }, true);
      return;
    }

    if (!Database.TryAddJoinRole(role.Id))
    {
      await command.RespondAsync(new DiscordEmbedBuilder
      {
        Color = DiscordColor.Red,
        Description = "Failed to add join role."
      }, true);
      return;
    }

    Logger.Log($"{command.Member.Username} ({command.Member.Id}) added the '{role.Name}' join role.");
    await command.RespondAsync(new DiscordEmbedBuilder
    {
      Color = DiscordColor.Green,
      Description = "Join role added."
    }, true);
  }
}