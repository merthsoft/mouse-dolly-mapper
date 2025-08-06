using UnityEngine;
using Verse;

namespace Merthsoft.MouseDollyMapper;

public class MouseButtonTracker(Game _) : GameComponent
{
    private static bool ButtonHeld = false;
    private static bool DragStart = false;
    private static Vector2 LastPosition = Vector2.zero;
    private static int ButtonDownTick = -1;

    public override void GameComponentUpdate()
    {
        bool currentlyHeld;
        try
        {
            currentlyHeld = Input.GetMouseButton(MouseDollyMapper.MouseDollyButton);
        }
        catch
        {
            currentlyHeld = false;
        }

        if (currentlyHeld && !ButtonHeld)
        {
            ButtonDownTick = Find.TickManager.TicksGame;
        }
        else if (!currentlyHeld)
        {
            ButtonDownTick = -1;
            DragStart = false;
        }

        ButtonHeld = currentlyHeld;
    }

    public static bool IsMouseDragged()
    {
        if (!ButtonHeld || !DragDelayPassed())
            return false;

        var now = GetMousePosition();
        var moved = now != LastPosition;
        LastPosition = now;
        DragStart |= moved;

        return moved;
    }

    public static bool HasDragged()
        => ButtonHeld && DragStart && DragDelayPassed();

    private static bool DragDelayPassed()
    {
        if (ButtonDownTick < 0)
            return false;

        return (Find.TickManager.TicksGame - ButtonDownTick) >= MouseDollyMapper.DragDelayTicks;
    }

    private static Vector2 GetMousePosition()
        => Event.current != null ? Event.current.mousePosition : (Vector2)Input.mousePosition;
}
