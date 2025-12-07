# RoleBoi [![Build Status](https://jenkins.karlofduty.com/job/DiscordBots/job/RoleBoi/job/main/badge/icon)](https://jenkins.karlofduty.com/blue/organizations/jenkins/DiscordBots%2FRoleBoi/activity) [![Release](https://img.shields.io/github/release/KarlofDuty/RoleBoi.svg)](https://github.com/KarlOfDuty/RoleBoi/releases) [![Discord Server](https://img.shields.io/discord/430468637183442945.svg?label=discord)](https://discord.gg/C5qMvkj)

This is a small Discord bot that helps with simple role management. It features functions to:

- Grant roles to all users when they join the Discord server.
- Track roles when users leave and give them back when they rejoin the server. This is useful for permission negating roles such as a "Muted" role.
- Create a selection box that users can use to grant roles to themselves.
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
| `/listjoinrole <role>`<br>`/listtrackedrole <role>`<br>`/listselectablerole <role>`<br>`/listpingrole <role>`         | Lists the roles in their respective list.                                                                         |
| `/removejoinrole <role>`<br>`/removetrackedrole <role>`<br>`/removeselectablerole <role>`<br>`/removepingrole <role>` | Removes a role from their respective list.                                                                        |
| `/createroleselector`                                                                                                 | Creates a role selector message which users can use to join any role added with the `/addselectablerole` command. |
| `/ping <role>`                                                                                                        | Pings a role added with `/addpingrole` even if it would normally be un-pingable by the user.                      |

### Config:

```yaml
bot:
  # Bot token.
  token: "<add-token-here>"

  # Decides which messages are shown in console
  # Possible values are: Critical, Error, Warning, Information, Debug.
  console-log-level: "Information"

  # Sets the type of activity for the bot to display in its presence status
  # Possible values are: Playing, Streaming, ListeningTo, Watching, Competing
  presence-type: "Watching"

  # Sets the activity text shown in the bots status
  presence-text: "Discord"

  # Log all console output to a file, can be overridden using command line arguments. Set to "" to disable.
  # When running the bot as a service this will be set to "/var/log/roleboi/roleboi.log" by the service.
  # The log file will still log all log levels regardless of the console log level setting.
  log-file: ""

  # Path to where the sqlite database file is saved.
  # When running the bot as a service this will be set to "/var/lib/roleboi/roleboi.db" by the service.
  database-file: "./roleboi.db"
```
