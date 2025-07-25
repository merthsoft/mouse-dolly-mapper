using UnityEngine;
using Verse;

namespace Merthsoft.MouseDollyMapper;

public class MouseButtonTracker : GameComponent
{
    public static readonly bool[] ButtonHeld = new bool[5];

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
}