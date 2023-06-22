# Save Recovery

Recovers some corrupted saves. Uninstall after use.

Install on the server (modding [guide](https://youtu.be/WfvA5a5tNHo)).

# Explanation

The Valheim update that optimized the save files reduced the amount of data that a single object can have. This has lead to some saves becoming corrupted.

The corruption happens when the save file is converted to the much smaller new format. On the next load, the game tries to read the corrupted data and crashes.

## Mistlands dungeons

Some dungeons can generate with over 126 rooms. Since each room saves two values, 127 rooms overflows the 255 value limit.

This mod automatically detects too big dungeons and removes extra rooms. The detection works even after the save has been corrupted.

The fix causes the dungeon to have holes in it, but it should be playable. After Hildir update, the Upgrade World mod can be used to regenerate the zone to restore the dungeon.

## Modded creatures

Each spawn entry is stored on the zone object. Installing too many creature mods or otherwise adding more spawn entries can corrupt the save if the 255 value limit is reached.

This typically happens when testing different mods so reseting the zone objects usually fixes the issue.

However if the issue persists, you may have to reduce the amount of spawns.


