# Skill Distribution
A mod for SPT that adds new functionalities to skill progression. Once you hit elite level on any skill, you can still earn XP in this category. Doing so distributes experience that you would gain to other non-elite skills according to your settings. Works with Fika.

More detailed description and screenshots are available on [SPT Forge](https://forge.sp-tarkov.com/mod/2144/skill-distribution).

## Installation
1. Make sure that both SPT Client and SPT Server are not running
2. Head to [releases page](https://github.com/danx91/SkillDistribution/releases)
3. Download correct version for your SPT
4. Open zip file
5. Drag and drop `BepInEx` and `SPT` folders to your SPT directory
6. Start server and client and make sure that mod is working

## Building from source
1. Download this project
2. Download [Common Library](https://github.com/danx91/SPT-ZGFueDkxCommonLibrary)
3. Update `.csproj` files to match your folder layout
4. Compile solution

## Config

### Server config
You can change the server config in `SPT/user/mods/SkillDistribution/config/config.jsonc`. Open and edit it with text editor of your choice. All configuration options are explained in comments in the config file.

### Client config
You can access config while in-game by pressing `F12` key and then selecting `SkillDistribution` tab

![config menu](/images/settings.png)

* **Experience distribution mode** - Determines how skill experience is distributed
* **Skills count** - Number of skills to distribute experience to
* **Allow gym** - Whether or not XP from gym should be also distributed if strength/endurance is maxed
* **Use bonuses** - Whether or not distributed XP used target skill bonuses
* **Use effectiveness** - Whether or not distributed XP use and cause traget skill fatigue
* **Cause fatigue** - Whether or not distributed XP cause target skill fatigue when use_effectiveness is false. This option has no effect if use effectiveness is set to true
* **Experience multiplier** - Experience multiplier of distributed XP. Use it to increase or decrease XP that is distributed. Cumulates with per-skill multipliers
* **Experience multiplier (gym)** - Experience multiplier of distributed XP from workout
* **Enable multipliers** - Whether to apply per-skill multipliers (doesn't work with workout)
* **\[skill\] multiplier** - Distributed XP multiplier when \[skill\] is source of the XP. Cumulates with global experience multiplier
* **Reset to server values** - Pull settings from server and apply them


## License
Copyright © 2025 danx91 (aka ZGFueDkx)

This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this program. If not, see https://www.gnu.org/licenses/.

If you believe that this software infringes yours or someone else's copyrights, please contact me via Discord to resolve this issue: **danx91**.