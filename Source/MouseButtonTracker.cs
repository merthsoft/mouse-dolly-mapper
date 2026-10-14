using System.Diagnostics;
using UnityEngine;
using Verse;

namespace Merthsoft.MouseDollyMapper;

public class MouseButtonTracker : GameComponent
{
    private static bool ButtonHeld;
    private static bool DragStart;
    private static Vector2 LastPosition;
    private static Vector2 CurrentDelta;
    private static long ButtonDownTime;

    public MouseButtonTracker(Game game) => Reset();

    public static void Reset()
    {
        ButtonHeld = DragStart = false;
        CurrentDelta = Vector2.zero;
    }

    public override void GameComponentUpdate()
    {
        var currentlyHeld = MouseDollyMapper.CanDolly && MouseDollyMapper.BindingHeld();
        if (currentlyHeld && !ButtonHeld)
        {
            ButtonDownTime = Stopwatch.GetTimestamp();
            LastPosition = UI.MousePositionOnUIInverted;
            DragStart = false;
        }
        else if (!currentlyHeld)
            Reset();
        ButtonHeld = currentlyHeld;
    }

    public static bool IsMouseDragged()
    {
        CurrentDelta = Vector2.zero;
        if (!ButtonHeld || !MouseDollyMapper.CanDolly)
            return false;

        var now = UI.MousePositionOnUIInverted;
        var delta = now - LastPosition;
        LastPosition = now;
        if (!DragDelayPassed())
            return false;
        CurrentDelta = delta;
        DragStart |= delta != Vector2.zero;
        return delta != Vector2.zero;
    }

    public static Vector2 DollyDelta() => CurrentDelta;

    // Keep the origin current when vanilla blocks motion over a window or modal UI.
    public static void FinishCameraEvent() => LastPosition = UI.MousePositionOnUIInverted;

    public static bool HasDragged()
        => MouseDollyMapper.CanDolly && ButtonHeld && DragStart && DragDelayPassed();

    private static bool DragDelayPassed()
        => DelayPassed(Stopwatch.GetElapsedTime(ButtonDownTime).TotalMilliseconds, MouseDollyMapper.DragDelayMs);

    // Pure timing predicate: unaffected by paused simulation or simulation speed.
    internal static bool DelayPassed(double elapsedMs, int delayMs) => elapsedMs >= delayMs;
}
