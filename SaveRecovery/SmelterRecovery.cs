using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;

namespace SaveRecovery;


[HarmonyPatch(typeof(ZDO), nameof(ZDO.Load))]
public class SmelterRecovery
{
  static readonly int limit = 255;
  // Id could be acquired from zdo but transpiler is simpler this way.
  static void CheckCorruption(ZDOID id, ZPackage pkg, ZDO zdo)
  {
    if (!ZDOExtraData.s_ints.ContainsKey(id)) return;
    if (!ZDOExtraData.s_ints[id].ContainsKey(ZDOVars.s_queued)) return;
    var queued = ZDOExtraData.s_ints[id][ZDOVars.s_queued];
    if (queued <= limit) return;

    // Queue is not read in order so we must gather all data and select the first 255 items.
    Dictionary<int, string> items = new();
    if (ZDOExtraData.s_strings.ContainsKey(id))
    {
      foreach (var item in ZDOExtraData.s_strings[id])
        items[item.Key] = item.Value;
      // No idea what was loaded so just remove everything.
      ZDOHelper.Release(ZDOExtraData.s_strings, id);
    }

    // Strings are only used for the queue so casting can be used to check how much data was loaded.
    var loaded = (int)(byte)queued;
    for (var i = loaded; i < queued; i++)
      items[pkg.ReadInt()] = pkg.ReadString();

    // Data will be fully loaded.
    ZDOExtraData.Reserve(id, ZDOExtraData.Type.String, limit);
    // Write data back to package.
    for (var i = 0; i < limit; i++)
    {
      var hash = ("item" + i.ToString()).GetStableHashCode();
      if (!items.ContainsKey(hash) || items[hash] == "")
      {
        // This should never happen but happens for some worlds.
        // Technically could compress the queue but out of scope for this mod.
        ZLog.LogWarning($"Queued item {i} not found in {Helper.Print(zdo)}, this is not normal!");
        continue;
      }
      ZDOExtraData.Add(id, hash, items[hash]);
    }
    ZDOExtraData.Add(id, ZDOVars.s_queued, limit);

    // Log discarded items.
    Dictionary<string, int> discarded = new();
    for (var i = limit; i < queued; i++)
    {
      var hash = ("item" + i.ToString()).GetStableHashCode();
      if (!items.ContainsKey(hash) || items[hash] == "")
      {
        ZLog.LogWarning($"Queued item {i} not found in {Helper.Print(zdo)}, this is not normal!");
        continue;
      }
      var item = items[hash];
      if (!discarded.ContainsKey(item)) discarded[item] = 0;
      discarded[item] += 1;
    }
    var str = string.Join(", ", discarded.Select(x => $"{x.Value} {x.Key}"));
    ZLog.LogWarning($"Removed {discarded.Sum(x => x.Value)} items from {Helper.Print(zdo)}: {str}");
  }
  static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
  {
    return new CodeMatcher(instructions)
      .MatchForward(false, new CodeMatch(OpCodes.Ldc_I4_5), new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(ZDOExtraData), nameof(ZDOExtraData.RemoveIfEmpty), new[] { typeof(ZDOID), typeof(ZDOExtraData.Type) })))
      .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_1))
      .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
      .InsertAndAdvance(new CodeInstruction(OpCodes.Call, Transpilers.EmitDelegate(CheckCorruption).operand))
      .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
      .InsertAndAdvance(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ZDO), nameof(ZDO.m_uid))))
      .InstructionEnumeration();
  }
}