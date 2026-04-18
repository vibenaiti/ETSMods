# QuickStash

A fork of the original QuickStash 

### Chat Commands
- .stash -- Automatically compulsively counts into all nearby chests. Add a '9' or '*' to the end of the chest name in order to exclude it. Note that you can only use symbols in the chest name if you have the client mod installed. You can also set up a keybind if you have the client mod.
- .clearchests (.clc) -- Add an '8' or '-' to the end of a chest name to mark it for deletion. Any marked chests will have their inventories deleted when this command is run. No more dropping stuff on the ground and waiting for it to despawn!
- .clearinventory (.ci) -- Deletes everything in your inventory other than the 1st row. No more dropping stuff on the ground and waiting for it to despawn! (This command is disabled by default to prevent griefing during raids)


QuickStash can be installed using multiple methods depending on your needs. See installation methods below.

### Client and Server Installation (Default)
The default installation which requires the mod to be installed on the client and server. Uses the default keybind `G` to stash items.
- Install [BepInEx](https://v-rising.thunderstore.io/package/BepInEx/BepInExPack_V_Rising)
- Install [Bloodstone](https://v-rising.thunderstore.io/package/deca/Bloodstone)
- Extract _QuickStash.dll_ into _(VRising folder)/BepInEx/plugins_
- Extract _QuickStash.dll_ into _(VRising folder)/VRising_Server/BepInEx/plugins_

### Client and Server Installation using VampireCommandFramework (Optional)
Optional installation which requires the mod to be installed on the client and server. Uses the default keybind `G` or the `.stash` command to stash items.
- Install [BepInEx](https://v-rising.thunderstore.io/package/BepInEx/BepInExPack_V_Rising)
- Install [Bloodstone](https://v-rising.thunderstore.io/package/deca/Bloodstone)
- Install [VampireCommandFramework](https://v-rising.thunderstore.io/package/deca/VampireCommandFramework/)
- Extract _QuickStash.dll_ into _(VRising folder)/BepInEx/plugins_
- Extract _QuickStash.dll_ into _(VRising folder)/VRising_Server/BepInEx/plugins_

### Server Only Installation using VampireCommandFramework (Optional)
Optional installation which is **server side only**. Uses the `.stash` command to stash items.
- Install [BepInEx](https://v-rising.thunderstore.io/package/BepInEx/BepInExPack_V_Rising) on the server
- Install [Bloodstone](https://v-rising.thunderstore.io/package/deca/Bloodstone) on the server
- Install [VampireCommandFramework](https://v-rising.thunderstore.io/package/deca/VampireCommandFramework/) on the server
- Extract _QuickStash.dll_ into _(VRising folder)/VRising_Server/BepInEx/plugins_

#### Optional
Singleplayer requires [ServerLaunchFix](https://v-rising.thunderstore.io/package/Mythic/ServerLaunchFix/) to fix issues with the server mods not working.

### Configuration

The keybinding can be changed in the in-game controls menu.

For server configuration, after running the game once, the config file will be generated.

- Update server config in _(VRising folder)/VRising_Server/BepInEx/config/QuickStash.cfg_

### Troubleshooting

- If the mod doesn't work, it may be because it is not installed on the server. Check your BepInEx logs on both the client and server to make sure the latest version of both QuickStash and Bloodstone where loaded.

### Support
- Open an issue on [github](https://github.com/iZastic/QuickStash/issues)
- Ask in the V Rising Mod Community [discord](https://vrisingmods.com/discord)

### Contributors
- willis: `willis#0575` on Discord
- iZastic: `@iZastic` on Discord
- Elmegaard: `Elmegaard#` on Discord
- Dresmyr: `@小爛土` on Discord

### Changelog
`1.4.2`
- Added a command to clear 

`1.4.1`
- Added a command to clear inventory

`1.4.0`
- Added exclusions for crafting stations / specially named chests

`1.3.3`
- Added support for optional VCF command

<details>

`1.3.2`
- Moved from Wetstone to Bloodstone

`1.3.1`
- Added support for bags

`1.3.0`
- Upgrade for Gloomrot

`1.2.3`
- Upgrade to Wetstone 1.1.0
- Potentially fixed rare client crash
- Fixed silver debuff not getting removed

`1.2.2`
- Reduce cooldown from 2 seconds to 0.5 seconds

`1.2.1`
- Fixed Readme

`1.2.0`
- Increased default range to 50
- Added Wetstone (keybinds added to controls in-game)
- Code refactor
- Fixed memory leak (but added small stutter when depositing)

`1.1.2`
- Fixed a client crash

`1.1.1`
- Updated Readme

`1.1.0`
- Set max distance
- Made config for keybind
- Made config for max distance

`1.0.1`
- Updated Readme

`1.0.0`
- Initial mod upload

</details>
