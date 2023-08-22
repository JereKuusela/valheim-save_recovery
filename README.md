# Save Recovery

Recovers some corrupted saves. Uninstall after use.

Install on the server (modding [guide](https://youtu.be/WfvA5a5tNHo)).

## Features

* Prevents the save corruption when converting old worlds to the new save format.
* Fixes dungeons on already corrupted saves.
* Possibly fixes overfilled crafting structures on already corrupted saves.

## Save conversion

The Valheim update that optimized the save files reduces the amount of data that a single object can have. This has lead to some saves becoming corrupted.

The corruption happens when the save file is converted to the new much smaller new format. On the next load, the game tries to read the corrupted data and crashes.

When converting an old save, this mod automatically removes data from objects which could corrupt the save.

### Mistlands dungeons

Some dungeons can generate with over 126 rooms. Since each room saves two values, 127 rooms overflows the 255 value limit.

This mod can read the corrupted room data and convert it to the new format (introduced in the Hildir update).

## Overfilled crafting stations

Some mods allow increasing the amount of items that can be queued inside crafting stations. 256 or more items in the queue overflows the 255 value limit.

After corruption, this can only be fixed if the crafting station was full.

This mod removes extra items past the 255 limit and prints removed items to the console.

Before using this mod, it is important to fix the configuration so that the structures can't get more than 255 items. Otherwise the save will get corrupted again.

### Modded creatures

Each spawn entry is stored on the zone object. Installing too many creature mods or otherwise adding more spawn or raid entries can corrupt the save if the 255 value limit is reached.

This typically happens when testing different mods so reseting the zone objects usually fixes the issue. However if the issue persists, you may have to reduce the amount of spawns.

This can't be fixed after corruption.

## Credits

Sources: [GitHub](https://github.com/JereKuusela/valheim-save_recovery)

Donations: [Buy me a computer](https://www.buymeacoffee.com/jerekuusela)
