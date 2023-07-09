# Save Recovery

Recovers some corrupted saves. Uninstall after use.

Install on the server (modding [guide](https://youtu.be/WfvA5a5tNHo)).

# Features

* Prevents the save corruption when converting old worlds to the new save format.
* Fixes dungeons on already corrupted saves.
* Fixes overfilled crafting structures on already corrupted saves.

# Save conversion

The Valheim update that optimized the save files reduces the amount of data that a single object can have. This has lead to some saves becoming corrupted.

The corruption happens when the save file is converted to the new much smaller new format. On the next load, the game tries to read the corrupted data and crashes.

When converting an old save, this mod automatically removes data from objects which would corrupt the save.

## Mistlands dungeons

Some dungeons can generate with over 126 rooms. Since each room saves two values, 127 rooms overflows the 255 value limit.

This mod removes extra rooms past the 126 limit.

The fix causes the dungeon to have holes in it, but it should be playable. After Hildir update, the Upgrade World mod can be used to regenerate the zone to restore the dungeon.

# Overfilled crafting structures

Some mods allow increasing the amount of items that can be stored in crafting structures. 256 or more items in the structure overflows the 255 value limit.

This mod removes extra items past the 255 limit and prints removed items to the console.

Before using this mod, it is important to fix the configuration so that the structures can't get more than 255 items. Otherwise the save will get corrupted again.

## Modded creatures

Each spawn entry is stored on the zone object. Installing too many creature mods or otherwise adding more spawn or raid entries can corrupt the save if the 255 value limit is reached.

This typically happens when testing different mods so reseting the zone objects usually fixes the issue. However if the issue persists, you may have to reduce the amount of spawns.

This mod removes every timestamp value for corrupted zone objects, which resets the system. Gracefully removing extra timestamps is not possible because new entries could be added which would corrupt the zone object again.

## Other

For other objects all of the values are removed. This is the safest approach because no way to know how many of the removes values get added back.


# After corruption

Usually already corrupted saves can't be recovered because objects are stored as a continous stream to the save file. Corrupted objects messes up the stream so the save system doesn't know which data belongs to which object.

However some things can be detected and fixed.

Corrupted dungeons can be detected because the amount of rooms is saved. This value can be used to discard extra rooms even for already corrupted saves.

Corrupted crafting structures can be detected because the amount of items is saved. This only works if the structure is full because an entry is saved also for already used items. If more data is saved than the amount of items, then there is no way simple way to fix the corruption (but might be still doable, just let me know if you suspect this is the case).


# Credits

Sources: [GitHub](https://github.com/JereKuusela/valheim-save_recovery)

Donations: [Buy me a computer](https://www.buymeacoffee.com/jerekuusela)
