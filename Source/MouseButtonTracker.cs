using UnityEngine;
using Verse;

namespace Merthsoft.MouseDollyMapper;

public class MouseButtonTracker : GameComponent
{
    public static readonly bool[] ButtonHeld = new bool[5];
    private static Vector2 lastPos = Vector2.zero;

    public MouseButtonTracker(Game _) { }

    public override void GameComponentUpdate()
    {
        for (int i = 0; i < ButtonHeld.Length; i++)
        {
            try
            {
                ButtonHeld[i] = Input.GetMouseButton(i);
            } catch
            {
                ButtonHeld[i] = false;
            }
        }
    }

    public static bool MouseDrag(int button)
    {
        if (button < 0 || button >= ButtonHeld.Length)
            return false;

        if (!MouseButtonTracker.ButtonHeld[button])
            return false;

        var now = Event.current.mousePosition;
        var moved = now != lastPos;
        lastPos = now;

        return moved;
    }
}