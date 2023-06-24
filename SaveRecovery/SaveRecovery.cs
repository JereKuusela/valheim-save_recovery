using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace SaveRecovery;
[BepInPlugin(GUID, NAME, VERSION)]

public class SaveRecovery : BaseUnityPlugin
{
  public const string GUID = "save_recovery";
  public const string NAME = "Save Recovery";
  public const string VERSION = "1.0";

  public void Awake()
  {
    new Harmony(GUID).PatchAll();
  }
}
