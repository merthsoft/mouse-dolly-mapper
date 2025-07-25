using UnityEngine;
using Verse;

namespace Merthsoft.MouseDollyMapper;

public static class ExtendedMouseDrag
{
    private static Vector2 lastPos = Vector2.zero;

    public static bool MouseDrag(int button)
    {
        if (!MouseButtonTracker.ButtonHeld[button])
            return false;

        Vector2 now = Event.current.mousePosition;
        bool moved = now != lastPos;
        lastPos = now;

        return moved;
    }
}
