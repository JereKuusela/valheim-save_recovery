using HarmonyLib;

namespace SaveRecovery;


[HarmonyPatch(typeof(ZDO), nameof(ZDO.LoadOldFormat))]
public class SaveConvert {

  static void Postfix(ZDO __instance) {
    var zdo = __instance;
    CleanExtraRooms(zdo);
    if (ZDOExtraData.s_ints.TryGetValue(zdo.m_uid, out var ints) && ints.length > 255) {
      ZLog.LogWarning($"Removed all ints from {Helper.Print(zdo)}");
      ZDOHelper.Release(ZDOExtraData.s_ints, zdo.m_uid);
    }
    if (ZDOExtraData.s_vec3.TryGetValue(zdo.m_uid, out var vecs) && vecs.length > 255) {
      ZLog.LogWarning($"Removed all vec3s from {Helper.Print(zdo)}");
      ZDOHelper.Release(ZDOExtraData.s_vec3, zdo.m_uid);
    }
    if (ZDOExtraData.s_quats.TryGetValue(zdo.m_uid, out var quats) && quats.length > 255) {
      ZLog.LogWarning($"Removed all quats from {Helper.Print(zdo)}");
      ZDOHelper.Release(ZDOExtraData.s_quats, zdo.m_uid);
    }
    if (ZDOExtraData.s_longs.TryGetValue(zdo.m_uid, out var longs) && longs.length > 255) {
      ZLog.LogWarning($"Removed all longs from {Helper.Print(zdo)}");
      ZDOHelper.Release(ZDOExtraData.s_longs, zdo.m_uid);
    }
    if (ZDOExtraData.s_floats.TryGetValue(zdo.m_uid, out var floats) && floats.length > 255) {
      ZLog.LogWarning($"Removed all floats from {Helper.Print(zdo)}");
      ZDOHelper.Release(ZDOExtraData.s_floats, zdo.m_uid);
    }
    if (ZDOExtraData.s_strings.TryGetValue(zdo.m_uid, out var strings) && strings.length > 255) {
      ZLog.LogWarning($"Removed all strings from {Helper.Print(zdo)}");
      ZDOHelper.Release(ZDOExtraData.s_strings, zdo.m_uid);
    }
    if (ZDOExtraData.s_byteArrays.TryGetValue(zdo.m_uid, out var bytes) && bytes.length > 255) {
      ZLog.LogWarning($"Removed all byte arrays from {Helper.Print(zdo)}");
      ZDOHelper.Release(ZDOExtraData.s_byteArrays, zdo.m_uid);
    }
  }


  static readonly int maxRooms = 255 / 2;
  static void CleanExtraRooms(ZDO zdo) {
    var rooms = zdo.GetInt(ZDOVars.s_rooms);
    if (rooms <= maxRooms) return;

    for (var i = maxRooms; i < rooms; i++) {
      var text = "room" + i.ToString();
      var hash = text.GetStableHashCode();
      ZDOHelper.Remove(ZDOExtraData.s_ints, zdo.m_uid, hash);
      hash = (text + "_seed").GetStableHashCode();
      ZDOHelper.Remove(ZDOExtraData.s_ints, zdo.m_uid, hash);
      hash = (text + "_pos").GetStableHashCode();
      ZDOHelper.Remove(ZDOExtraData.s_vec3, zdo.m_uid, hash);
      hash = (text + "_rot").GetStableHashCode();
      ZDOHelper.Remove(ZDOExtraData.s_quats, zdo.m_uid, hash);
    }
    ZLog.LogWarning($"Removed {rooms - maxRooms} extra rooms from {Helper.Print(zdo)}");
  }
}