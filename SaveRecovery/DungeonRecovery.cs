using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;
using HarmonyLib;

namespace SaveRecovery;


[HarmonyPatch(typeof(ZDO), nameof(ZDO.Load))]
public class DungeonRecovery
{
  static readonly int maxRooms = 255 / 2;
  // Id could be acquired from zdo but transpiler is simpler this way.
  static void CheckCorruption(ZDOID id, ZPackage pkg, ZDO zdo)
  {
    // Dungeons save one vector for each room.
    // This way we can find zdos that have more rooms than the max.
    var rooms = ZDOExtraData.s_vec3.ContainsKey(id) ? ZDOExtraData.s_vec3[id].Count : 0;
    if (rooms <= maxRooms) return;
    // On save, the amount of data gets casted to a byte. This means that arbitrary amount of data will be loaded.
    var loaded = ZDOExtraData.s_ints.ContainsKey(id) ? ZDOExtraData.s_ints[id].Count : 0;
    // All data can be just loaded because it gets converted to a working format.
    var entries = 1 + 2 * rooms;
    ZDOExtraData.Reserve(id, ZDOExtraData.Type.Int, entries);
    for (var i = loaded; i < entries; i++)
      ZDOExtraData.Add(id, pkg.ReadInt(), pkg.ReadInt());
    ZLog.LogWarning($"Fixed rooms for dungeon {Helper.Print(zdo)}");
  }
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions)
      .MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_3), new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(ZDOExtraData), nameof(ZDOExtraData.RemoveIfEmpty), new[] { typeof(ZDOID), typeof(ZDOExtraData.Type) })))
      .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_1))
      .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
      .InsertAndAdvance(new CodeInstruction(OpCodes.Call, Transpilers.EmitDelegate(CheckCorruption).operand))
      .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
      .InsertAndAdvance(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ZDO), nameof(ZDO.m_uid))))
      .InstructionEnumeration();
  }
  static void Postfix(ZDO __instance)
  {
    if (__instance.GetInt(ZDOVars.s_rooms) > 0) ZDOMan.instance.ConvertDungeonRooms([__instance]);
  }
}

[HarmonyPatch(typeof(ZDOExtraData), nameof(ZDOExtraData.Init))]
public class PreventRoomSeedDrop
{
  static void Postfix()
  {
    // This messes up the dungeon recovery because loaded becomes inaccurate.
    // Pointless anyway because dungeons get converted to the new format.
    for (int i = 0; i < 256; i++)
      ZDOHelper.s_stripOldData.Remove(("room" + i.ToString() + "_seed").GetStableHashCode());
  }
}