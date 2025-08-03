using HarmonyLib;
using Verse;
using RimWorld;

namespace Merthsoft.MouseDollyMapper;

[HarmonyPatch(typeof(ScreenshotModeHandler), nameof(ScreenshotModeHandler.FiltersCurrentEvent), MethodType.Getter)]
public static class ScreenshotModeHandlerFiltersCurrentEventPatch
{
    public static void Postfix(ref bool __result)
    {
        if (MouseDollyMapper.HideMainUiButtons())
        {
            __result = true;
        }
    }
}
