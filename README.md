# RoleBoi  [![Release](https://img.shields.io/github/release/KarlofDuty/RoleBoi.svg)](https://github.com/KarlOfDuty/RoleBoi/releases) ![GitHub commits since latest release](https://img.shields.io/github/commits-since/karlofduty/roleboi/latest) [![Discord Server](https://img.shields.io/discord/430468637183442945.svg?label=discord)](https://discord.gg/C5qMvkj) [![Build Status](https://jenkins.karlofduty.com/job/DiscordBots/job/RoleBoi/job/main/badge/icon)](https://jenkins.karlofduty.com/blue/organizations/jenkins/DiscordBots%2FRoleBoi/activity) [![Codacy Badge](https://app.codacy.com/project/badge/Grade/b67febd413394f0daacc71268e1ef288)](https://app.codacy.com/gh/KarlOfDuty/RoleBoi/dashboard) ![GitHub License](https://img.shields.io/github/license/karlofduty/roleboi)

This is a small Discord bot that helps with simple role management automation. It features functions to:

- Grant roles to all users when they join the Discord server.
- Track roles when users leave and give them back when they rejoin the server. This is useful for permission negating roles such as a "Muted" role.
- Create a selection box that users can use to grant roles to themselves on demand.
- A ping command that moderators can use to ping roles that are normally un-pingable.

All of these functions are completely independent of each other and can be completely turned off by simply turning the related commands off in the Discord settings.

## Setup

1. [Create a new bot application](https://discordpy.readthedocs.io/en/latest/discord.html).

2. Download the bot executable for your operating system, either a [release version](https://github.com/KarlOfDuty/RoleBoi/releases) or a [dev build](http://95.217.45.17:8080/blue/organizations/jenkins/RoleBoi/activity). The bot should include dotnet but if your bot doesnt work and this seems to be the issue please tell me.

3. Run the bot executable once to generate the config.

4. Set up the config (`config.yml`) to your specifications, there are instructions inside and also further down on this page. If you need more help either contact me in Discord or through an issue here.

## Commands
| Command                                                                                                               | Description                                                                                                       |
|-----------------------------------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------|
| `/addjoinrole <role>`<br>`/addtrackedrole <role>`<br>`/addselectablerole <role>`<br>`/addpingrole <role>`             | Adds a role to their respective list.                                                                             |
| `/listjoinroles <role>`<br>`/listtrackedroles <role>`<br>`/listselectableroles <role>`<br>`/listpingroles <role>`     | Lists the roles in their respective list.                                                                         |
| `/removejoinrole <role>`<br>`/removetrackedrole <role>`<br>`/removeselectablerole <role>`<br>`/removepingrole <role>` | Removes a role from their respective list.                                                                        |
| `/createroleselector`                                                                                                 | Creates a role selector message which users can use to join any role added with the `/addselectablerole` command. |
| `/ping <role>`                                                                                                        | Pings a role added with `/addpingrole` even if it would normally be un-pingable by the user.                      |

