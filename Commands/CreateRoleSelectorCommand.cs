using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using RoleBoi;

namespace RoleBoi.Commands;

public class CreateRoleSelectorCommand
{
  [RequireGuild]
  [Command("createroleselector")]
  [Description("Creates a selection box which users can use to get new roles.")]
  public async Task OnExecute(SlashCommandContext command,
    [Parameter("placeholder")] [Description("(Optional) The message to show in the selection box.")] string message = null)
  {
    List<DiscordSelectComponent> components = await GetSelectComponents(command, message ?? "Join/Leave role");

    if (components.Count == 0)
    {
      await command.RespondAsync(new DiscordEmbedBuilder
      {
        Color = DiscordColor.Red,
        Description = "There are no roles registered for the selector, add some using `/addselectablerole`."
      }, true);
      return;
    }

    DiscordMessageBuilder builder = new DiscordMessageBuilder()
      .WithContent(" ")
      .AddActionRowComponent(new DiscordActionRowComponent(components));

    await command.Channel.SendMessageAsync(builder);
    await command.RespondAsync(new DiscordEmbedBuilder
    {
      Color = DiscordColor.Green,
      Description = "Successfully created message, make sure to run this command again if you add new roles to the bot."
    }, true);

    Logger.Log($"{command.Member.Username} ({command.Member.Id}) created a role selector in channel '{command.Channel.Name}' ({command.Channel.Id}).");
  }

  public static async Task<List<DiscordSelectComponent>> GetSelectComponents(SlashCommandContext command, string placeholder)
  {
    List<ulong> selectableRoles = Database.GetSelectableRoles();

    List<DiscordRole> savedRoles = command.Guild.Roles.Where(rolePair => selectableRoles.Contains(rolePair.Key))
                                                      .Select(rolePair => rolePair.Value).ToList();

    savedRoles = savedRoles.OrderBy(x => x.Name).ToList();
    List<DiscordSelectComponent> selectionComponents = new List<DiscordSelectComponent>();
    int selectionOptions = 0;
    for (int selectionBoxes = 0; selectionBoxes < 5 && selectionOptions < savedRoles.Count; selectionBoxes++)
    {
      List<DiscordSelectComponentOption> roleOptions = new List<DiscordSelectComponentOption>();

      for (; selectionOptions < 25 * (selectionBoxes + 1) && selectionOptions < savedRoles.Count; selectionOptions++)
      {
        roleOptions.Add(new DiscordSelectComponentOption(savedRoles[selectionOptions].Name, savedRoles[selectionOptions].Id.ToString()));
      }
      selectionComponents.Add(new DiscordSelectComponent("roleboi_togglerole" + selectionBoxes, placeholder, roleOptions, false, 0, 1));
    }

    return selectionComponents;
  }
}