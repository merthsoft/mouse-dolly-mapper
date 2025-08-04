using HarmonyLib;
using Verse;
using RimWorld;

namespace Merthsoft.MouseDollyMapper.Patches;

[HarmonyPatch(typeof(ScreenshotModeHandler), nameof(ScreenshotModeHandler.FiltersCurrentEvent), MethodType.Getter)]
public static class ScreenshotModeHandler_FiltersCurrentEvent
{
    public static void Postfix(ref bool __result)
    {
        if (MouseDollyMapper.HideMainUiButtons())
            __result = true;
    }
}
