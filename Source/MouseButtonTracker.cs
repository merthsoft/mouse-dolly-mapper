using UnityEngine;
using Verse;

namespace Merthsoft.MouseDollyMapper;

public class MouseButtonTracker : GameComponent
{
    private static bool ButtonHeld = false;
    private static bool DragStart = false;
    private static Vector2 lastPos = Vector2.zero;

    public MouseButtonTracker(Game _) { }

    public override void GameComponentUpdate()
    {
        try
        {
            ButtonHeld = Input.GetMouseButton(MouseDollyMapper.MouseDollyButton);
        } catch
        {
            ButtonHeld = false;
        }

        if (!ButtonHeld)
            DragStart = false;
    }

    public static bool IsMouseDragged()
    {
        if (!ButtonHeld)
            return false;

        var now = Event.current.mousePosition;
        var moved = now != lastPos;
        lastPos = now;
        DragStart |= moved;
        if (moved)
            Event.current.Use();

        return moved;
    }

    public static bool HasDragged()
        => ButtonHeld && DragStart;
}