% roleboi(1) | User Commands
%
% "2025-05-25"

# NAME

roleboi - A small role management Discord bot.

# SYNOPSIS

**roleboi** [**-c** *CONFIG*] [**-l** *PATH*] [**--leave**=*ID,ID,...*]  
**roleboi** [**-h** | **--help**]  
**roleboi** [**-v** | **--version**]

# DESCRIPTION

**roleboi** retains specific Discord roles if users rejoin the server. TODO: Update This!
Useful for muted roles or other permission negating roles.
Leaving members are saved in a mysql database with all tracked roles they had when they left.

# OPTIONS

**-c**, **--config**=*PATH*
:   Specify an alternative configuration file to use.

**-l**, **--log-path**=*PATH*
:   Set the log file path.

**--leave**=*ID[,ID...]*
:   Make the bot leave specific Discord servers using their server IDs.

**-h**, **--help**
:   Show help message and exit.

**-v**, **--version**
:   Show version information and exit.

# CONFIGURATION

The bot configuration uses the YAML format (<https://yaml.org>).

A default configuration file is created automatically if none exists when the bot starts.

It is fully commented and includes documentation for each configuration option.

# DATABASE

The bot requires a MySQL/MariaDB database to store most of its data.

Requirements:

- A database for the bot to store its data in.
- A user with full permissions on the database, and ideally no permissions on anything else.

# FILES

*/etc/roleboi/config.yml*
:   System-wide configuration file, used when running as a service.

*/var/log/roleboi/roleboi.log*
:   System-wide log file, used when running as a service.

# EXIT STATUS

- **0** Success

- **1** Error

# EXAMPLES

Start the bot with the configuration file and transcripts in the current working directory:

    roleboi

Specify a custom configuration file and transcripts directory:

    roleboi --config /path/to/config.yml --transcripts /path/to/transcripts

Run the bot as the roleboi user using default system paths:

    sudo -u "roleboi" roleboi --config /etc/roleboi/config.yml

Start the bot using the included systemd service:

    sudo systemctl start roleboi

Make the bot leave specific servers:

    roleboi --leave 123456789012345678,987654321098765432

# COPYRIGHT

Copyright © 2025 Karl Essinger

This software is licensed under the GNU General Public License version 3.0 (GPLv3).

On Debian systems, the complete text of the GNU General Public License v3.0
can be found in /usr/share/common-licenses/GPL-3.

Otherwise, see <https://www.gnu.org/licenses/gpl-3.0.html>.

# BUGS

Report bugs at the project's issue tracker:  
<https://github.com/KarlOfDuty/RoleBoi/issues>

# AUTHOR

Karl Essinger <xkaess22@gmail.com>