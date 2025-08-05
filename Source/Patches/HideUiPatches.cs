using HarmonyLib;
using RimWorld;
using Verse;

namespace Merthsoft.MouseDollyMapper.Patches;

[HarmonyPatch(typeof(Window), nameof(Window.WindowOnGUI))]
public static class Window_WindowOnGUI
{
    static bool Prefix(Window __instance)
    {
        if (__instance is MainTabWindow && MouseDollyMapper.HideOpenTab())
            return false;

        return true;
    }
}

[HarmonyPatch(typeof(WindowStack), nameof(WindowStack.WindowStackOnGUI))]
public static class WindowStack_WindowStackOnGUI
{
    static bool Prefix()
    {
        if (MouseDollyMapper.HideOpenTab())
            return false;

        if (MouseDollyMapper.HideMainUiButtons())
            return false;

        return true;
    }
}

[HarmonyPatch(typeof(DesignatorManager), nameof(DesignatorManager.DesignationManagerOnGUI))]
public static class DesignatorManager_DesignationManagerOnGUI
{
    static bool Prefix()
    {
        if (MouseDollyMapper.HideOpenTab())
            return false;
        return true;
    }
}

[HarmonyPatch(typeof(GizmoGridDrawer), nameof(GizmoGridDrawer.DrawGizmoGrid))]
public static class GizmoGridDrawer_DrawGizmoGrid
{
    static bool Prefix()
    {
        if (MouseDollyMapper.HideOpenTab())
            return false;
        return true;
    }
}
