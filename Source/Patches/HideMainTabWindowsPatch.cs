using HarmonyLib;
using Verse;

namespace Merthsoft.MouseDollyMapper;

[HarmonyPatch(typeof(WindowStack), nameof(WindowStack.WindowStackOnGUI))]
public static class HideMainTabsInWindowStack
{
    static bool Prefix()
    {
        return !MouseDollyMapper.HideMainUiButtons();
    }
}
