using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace SaveRecovery;
[BepInPlugin(GUID, NAME, VERSION)]

[HarmonyPatch(typeof(ZDO), nameof(ZDO.Load))]
public class SaveRecovery : BaseUnityPlugin
{
  public const string GUID = "save_recovery";
  public const string NAME = "Save Recovery";
  public const string VERSION = "1.0";
  public void Awake()
  {
    new Harmony(GUID).PatchAll();
  }
  static readonly int maxRooms = 255 / 2;
  static void CheckCorruption(ZDOID id, ZPackage pkg)
  {
    // Dungeons save one vector for each room.
    // This way we can find zdos that have more rooms than the max.
    var rooms = ZDOExtraData.s_vec3.ContainsKey(id) ? ZDOExtraData.s_vec3[id].Count : 0;
    if (rooms <= maxRooms) return;
    // On save, the amount of data gets casted to a byte. This means that arbitrary amount of data will be loaded.
    var loaded = ZDOExtraData.s_ints.ContainsKey(id) ? ZDOExtraData.s_ints[id].Count : 0;

    // Read up to the max (-1 to check for rooms).
    for (var i = loaded; i < 254; i++)
      ZDOExtraData.Add(id, pkg.ReadInt(), pkg.ReadInt());
    var isRoomsSet = ZDOExtraData.s_ints.ContainsKey(id) && ZDOExtraData.s_ints[id].ContainsKey(ZDOVars.s_rooms);
    // If rooms is not set, we can't read the last one because setting the rooms would then overflow.
    if (isRoomsSet)
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
    ZDOExtraData.Set(id, ZDOVars.s_rooms, maxRooms);
    ZLog.Log($"Destroyed {rooms - maxRooms} rooms from the dungeon {id}");
  }
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions)
      .MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_3), new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(ZDOExtraData), nameof(ZDOExtraData.RemoveIfEmpty), new[] { typeof(ZDOID), typeof(ZDOExtraData.Type) })))
      .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_1))
      .InsertAndAdvance(new CodeInstruction(OpCodes.Call, Transpilers.EmitDelegate(CheckCorruption).operand))
      .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
      .InsertAndAdvance(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ZDO), nameof(ZDO.m_uid))))
      .InstructionEnumeration();
  }
}
