using UnityEngine;
using Verse;

namespace Merthsoft.MouseDollyMapper;

public class MouseDollyMapperSettings : ModSettings
{
    public int MouseDollyButton = 2;
    public bool DisablePanningWhenColonistIsSelected = true;
    public bool DisableArchitectMenuOnRightMouse = true;

    public override void ExposeData()
    {
        Scribe_Values.Look(ref MouseDollyButton, "MouseDollyButton", 2);
        Scribe_Values.Look(ref DisablePanningWhenColonistIsSelected, "DisablePanningWhenColonistIsSelected", true);
        Scribe_Values.Look(ref DisableArchitectMenuOnRightMouse, "DisableArchitectMenuOnRightMouse", true);
    }
}
