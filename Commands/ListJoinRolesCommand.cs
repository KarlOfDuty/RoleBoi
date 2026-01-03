using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;

namespace RoleBoi.Commands;

public class ListJoinRolesCommand
{
  [RequireGuild]
  [Command("listjoinroles")]
  [Description("List all roles that are given to users when they join.")]
  public async Task OnExecute(SlashCommandContext command)
  {
    List<ulong> roleIDs = Database.GetJoinRoles();
    if (roleIDs.Count == 0)
    {
      await command.RespondAsync(new DiscordEmbedBuilder
      {
        Color = DiscordColor.Red,
        Description = "There are no join roles."
      }, true);
      return;
    }

    StringBuilder sb = new();
    foreach (ulong roleID in roleIDs)
    {
      if (command.Guild.Roles.TryGetValue(roleID, out DiscordRole role))
      {
        sb.AppendLine($"{role.Mention} ({role.Id})");
      }
      else
      {
        sb.AppendLine($"Deleted Role ({roleID})");
      }
    }

    await command.RespondAsync(new DiscordEmbedBuilder
    {
      Title = "Join Roles",
      Color = DiscordColor.Green,
      Description = sb.ToString()
    }, true);
  }
}
