namespace SaveRecovery;

public class Helper
{

  public static string Print(ZDO zdo)
  {
    var prefab = ZNetScene.instance.GetPrefab(zdo.m_prefab);
    var name = prefab ? prefab.name : "unknown";
    return $"{name} ({zdo.m_uid}) at {zdo.m_position:F0}";
  }
}
