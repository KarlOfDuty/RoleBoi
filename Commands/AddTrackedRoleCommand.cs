using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;

namespace RoleBoi.Commands;

public class AddTrackedRoleCommand
{
  [RequireGuild]
  [Command("addtrackedrole")]
  [Description("If users with this role leave the server they will get it back if they rejoin.")]
  public async Task OnExecute(SlashCommandContext command, [Parameter("role")] [Description("The role you want to add.")] DiscordRole role)
  {
    if (Database.GetTrackedRoles().Any(r => r == role.Id))
    {
      await command.RespondAsync(new DiscordEmbedBuilder
      {
        Color = DiscordColor.Red,
        Description = "That role is already tracked."
      }, true);
      return;
    }

    if (!Database.TryAddTrackedRole(role.Id))
    {
      await command.RespondAsync(new DiscordEmbedBuilder
      {
        Color = DiscordColor.Red,
        Description = "Failed to add tracked role."
      }, true);
      return;
    }

    Logger.Log($"{command.Member.Username} ({command.Member.Id}) added the '{role.Name}' tracked role.");
    await command.RespondAsync(new DiscordEmbedBuilder
    {
      Color = DiscordColor.Green,
      Description = "Tracked role added."
    }, true);
  }
}