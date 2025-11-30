# MuteBoi [![Build Status](https://jenkins.karlofduty.com/job/DiscordBots/job/MuteBoi/job/main/badge/icon)](https://jenkins.karlofduty.com/blue/organizations/jenkins/DiscordBots%2FMuteBoi/activity) [![Release](https://img.shields.io/github/release/KarlofDuty/MuteBoi.svg)](https://github.com/KarlOfDuty/MuteBoi/releases) [![Discord Server](https://img.shields.io/discord/430468637183442945.svg?label=discord)](https://discord.gg/C5qMvkj)
Retains specific Discord roles if users leave the server. Useful for muted roles or other permission negating roles. Leaving members are saved in a mysql database with all tracked roles they had when they left.

## Setup

1. [Create a new bot application](https://discordpy.readthedocs.io/en/latest/discord.html).

2. Download the bot executable for your operating system, either a [release version](https://github.com/KarlOfDuty/MuteBoi/releases) or a [dev build](http://95.217.45.17:8080/blue/organizations/jenkins/MuteBoi/activity). The bot should include dotnet but if your bot doesnt work and this seems to be the issue please tell me.

3. Run the bot executable once to generate the config.

4. Set up the config (`config.yml`) to your specifications, there are instructions inside and also further down on this page. If you need more help either contact me in Discord or through an issue here.

### Config:

```yaml
bot:
    # Bot token.
    token: "<add-token-here>"
    # Decides what messages are shown in console, possible values are: Critical, Error, Warning, Info, Debug.
    console-log-level: "Info"
    # A list of role ids that should be tracked
    tracked-roles:
      - 111111111111111111
      - 222222222222222222
      - 333333333333333333

database:
    # Address and port of the mysql server
    address: "127.0.0.1"
    port: 3306
    # Name of the database to use
    name: "muteboi"
    # Username and password for authentication
    user: ""
    password: ""
```
