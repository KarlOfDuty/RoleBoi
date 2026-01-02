using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using RoleBoi;

namespace RoleBoi.Commands;

public class CreateRoleSelectorCommand : ApplicationCommandModule
{
  [SlashRequireGuild]
  [SlashCommand("createroleselector", "Creates a selection box which users can use to get new roles.")]
  public async Task OnExecute(InteractionContext command)
  {
    DiscordMessageBuilder builder = new DiscordMessageBuilder().WithContent("Use this to join or leave public roles:");

    foreach (DiscordSelectComponent component in await GetSelectComponents(command))
    {
      builder.AddComponents(component);
    }

    if (!builder.Components.Any())
    {
      await command.CreateResponseAsync(new DiscordEmbedBuilder
      {
        Color = DiscordColor.Red,
        Description = "There are no roles registered for the selector, add some using `/addselectablerole`."
      }, true);
      return;
    }

    await command.Channel.SendMessageAsync(builder);
    Logger.Log($"{command.Member.Username} ({command.Member.Id}) created a role selector in channel '{command.Channel.Name}' ({command.Channel.Id}).");
    await command.CreateResponseAsync(new DiscordEmbedBuilder
    {
      Color = DiscordColor.Green,
      Description = "Successfully created message, make sure to run this command again if you add new roles to the bot."
    }, true);
  }

  public static async Task<List<DiscordSelectComponent>> GetSelectComponents(InteractionContext command)
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
      selectionComponents.Add(new DiscordSelectComponent("roleboi_togglerole" + selectionBoxes, "Join/Leave role", roleOptions, false, 0, 1));
    }

    return selectionComponents;
  }
}