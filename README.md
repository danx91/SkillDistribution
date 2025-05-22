# Skill Distribution
A mod for SPT that adds new functionalities to skill progression. Once you hit elite level on any skill, you can still earn XP in this category. Doing so distributes experience that you would gain to other non-elite skills according to your settings. Works with Fika.

More detailed description and screenshots are available on [SPT Hub](https://hub.sp-tarkov.com/files/file/2843-skill-distribution/).

## Installation
1. Make sure that both SPT Client and SPT Server are not running
2. Head to [releases page](https://github.com/danx91/SkillDistribution/releases)
3. Download correct version for your SPT
4. Open zip file
5. Drag and drop `BepInEx` and `user` folders to your SPT directory
6. Start server and client and make sure that mod is working

## Config

### Server config
You can change the server config in `user/mods/zgfuedkx-skilldistribution/config/config.jsonc`. Open and edit it with text editor of your choice. All configuration options are explained in comments in the config file.

### Client config
You can access config while in-game by pressing `F12` key and then selecting `SkillDistribution` tab

* **Experience distribution mode** - Determines how skill experience is distributed
* **Skills count** - Number of skills to distribute experience to
* **Allow gym** - Whether or not XP from gym should be also distributed if strength/endurance is maxed
* **Use bonuses** - Whether or not distributed XP used target skill bonuses
* **Use effectiveness** - Whether or not distributed XP use and cause traget skill fatigue
* **Cause fatigue** - Whether or not distributed XP cause target skill fatigue when use_effectiveness is false. This option has no effect if use effectiveness is set to true
* **Experience multiplier** - Experience multiplier of distributed XP. Use it to increase or decrease XP that is distributed
* **Experience multiplier (gym)** - Experience multiplier of distributed XP from workout
* **Reset to server values** - Pull settings from server and apply them


## License
Copyright © 2025 danx91 (aka ZGFueDkx)
This software is distributed under GNU GPLv3 license. Full license text can be found [here](LICENSE).

If you believe that this software infringes yours or someone else's copyrights, please contact me via Discord to resolve this issue: **danx91**.