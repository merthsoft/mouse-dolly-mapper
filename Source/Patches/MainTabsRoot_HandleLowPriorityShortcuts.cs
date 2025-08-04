using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using Verse;
using Verse.Steam;

namespace Merthsoft.MouseDollyMapper.Patches;

[HarmonyPatch(typeof(MainTabsRoot), nameof(MainTabsRoot.HandleLowPriorityShortcuts))]
public static class MainTabsRoot_HandleLowPriorityShortcuts
{
    public static bool Prefix()
        => Event.current.button != 1
        || MouseDollyMapper.MouseDollyButton != 1
        || !MouseDollyMapper.DisableArchitectMenuOnRightMouse;
}
