using Merthsoft.MouseDollyMapper.Enums;
using UnityEngine;
using Verse;

namespace Merthsoft.MouseDollyMapper;

public class MouseDollyMapperSettings : ModSettings
{
    public int MouseDollyButton = 2;
    public bool DisablePanningWhenColonistIsSelected = true;
    public ArchitectMenuMode ArchitectMenuMode = ArchitectMenuMode.Disable;
    public UiHidingMode UiHidingMode = UiHidingMode.AllUi;
    public int DragDelayMs = 0;

    public override void ExposeData()
    {
        Scribe_Values.Look(ref MouseDollyButton, "MouseDollyButton", 2);
        Scribe_Values.Look(ref DisablePanningWhenColonistIsSelected, "DisablePanningWhenColonistIsSelected", true);
        Scribe_Values.Look(ref ArchitectMenuMode, "ArchitectMenuMode", ArchitectMenuMode.ToggleOnClick);
        Scribe_Values.Look(ref UiHidingMode, "UiHidingMode", UiHidingMode.AllUi);
        Scribe_Values.Look(ref DragDelayMs, "DragDelayMs", 0);
    }
}
