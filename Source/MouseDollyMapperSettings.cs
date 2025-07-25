using UnityEngine;
using Verse;

namespace Merthsoft.MouseDollyMapper;

public class MouseDollyMapperSettings : ModSettings
{
    public int MouseDollyButton = 2;

    public override void ExposeData()
    {
        Scribe_Values.Look(ref MouseDollyButton, "MouseDollyButton", 2);
    }
}
