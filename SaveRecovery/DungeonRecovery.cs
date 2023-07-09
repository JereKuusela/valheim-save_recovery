using System.Collections.Generic;
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
    // Will be fully loaded.
    ZDOExtraData.Reserve(id, ZDOExtraData.Type.Int, 255);
    // Read up to the max (-1 to check for rooms).
    for (var i = loaded; i < 254; i++)
      ZDOExtraData.Add(id, pkg.ReadInt(), pkg.ReadInt());
    var isRoomsKey = ZDOExtraData.s_ints.ContainsKey(id) && ZDOExtraData.s_ints[id].ContainsKey(ZDOVars.s_rooms);
    // If rooms value doesn't exist, the last value can't be read,
    // Otherwise adding the rooms value would overflow the byte limit.
    if (isRoomsKey)
      ZDOExtraData.Add(id, pkg.ReadInt(), pkg.ReadInt());
    else
    {
      pkg.ReadInt();
      pkg.ReadInt();
    }

    // Discard the rest.
    var entries = 1 + 2 * rooms;
    for (var i = 255; i < entries; i++)
    {
      pkg.ReadInt();
      pkg.ReadInt();
    }
    ZDOExtraData.Add(id, ZDOVars.s_rooms, maxRooms);

    // Clean up vectors and quats from discarded rooms.
    // Otherwise this clean up code runs every time the save is loaded.
    var vecs = ZDOExtraData.s_vec3.ContainsKey(id) ? ZDOExtraData.s_vec3[id].Count : 0;
    for (var i = 0; i < vecs; i++)
    {
      var text = "room" + i.ToString();
      var hash = text.GetStableHashCode();
      if (ZDOExtraData.s_ints[id].ContainsKey(hash)) continue;
      hash = (text + "_pos").GetStableHashCode();
      ZDOHelper.Remove(ZDOExtraData.s_vec3, id, hash);
      // Assuming there are always more vectors than quats.
      // Quats can get removed when room rotation equals identity, but room position is never at zero point.
      hash = (text + "_rot").GetStableHashCode();
      ZDOHelper.Remove(ZDOExtraData.s_quats, id, hash);

    }
    ZLog.LogWarning($"Destroyed {rooms - maxRooms} rooms from {Helper.Print(zdo)}");
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
}