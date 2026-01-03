using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;

namespace RoleBoi.Commands;

public class PingCommand
{
  [RequireGuild]
  [Command("ping")]
  [Description("Mentions a Discord role.")]
  public async Task OnExecute(SlashCommandContext command, [Parameter("Role")] [Description("The role you want to mention.")] DiscordRole role)
  {
    if (Database.GetPingableRoles().All(savedRole => savedRole != role.Id))
    {
      await command.RespondAsync(new DiscordEmbedBuilder
      {
        Color = DiscordColor.Red,
        Description = "This role has not been set as pingable in the bot settings."
      }, true);
      return;
    }

    Logger.Log($"{command.Member.Username} ({command.Member.Id}) pinged the '{role.Name}' role.");
    await command.RespondAsync(role.Mention);
  }
}