using BepInEx;
using HarmonyLib;

namespace SaveRecovery;
[BepInPlugin(GUID, NAME, VERSION)]

public class SaveRecovery : BaseUnityPlugin
{
  public const string GUID = "save_recovery";
  public const string NAME = "Save Recovery";
  public const string VERSION = "1.2";

  public void Awake()
  {
    new Harmony(GUID).PatchAll();
  }
}
