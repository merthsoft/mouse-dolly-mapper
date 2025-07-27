using HarmonyLib;
using UnityEngine;
using Verse;
using Verse.Steam;

namespace Merthsoft.MouseDollyMapper;

public class MouseDollyMapper : Mod
{
    private static MouseDollyMapperSettings Settings;

    public static int MouseDollyButton => Settings.MouseDollyButton;

    public static bool DollyButtonPressed()
        => !SteamDeck.IsSteamDeck 
        && MouseButtonTracker.IsMouseDragged()
        && !ColonistSelectedCheck();

    private static bool ColonistSelectedCheck()
        => Settings.DisablePanningWhenColonistIsSelected && Find.Selector.AnyPawnSelected;

    public MouseDollyMapper(ModContentPack content) : base(content)
    {
        Settings = GetSettings<MouseDollyMapperSettings>();
        var harmony = new Harmony("Merthsoft.MouseDollyMapper");
        harmony.PatchAll();
    }

    public override string SettingsCategory() => "Mouse Dolly Mapper";

    private string ButtonName(int i) => i switch
    {
        0 => "Left (0)",
        1 => "Right (1)",
        2 => "Middle (2)",
        3 => $"Back (3)",
        4 => $"Forward (4)",
        _ => $"Mouse {i}",
    };

    private bool waitingForClick = false;
    public override void DoSettingsWindowContents(Rect inRect)
    {
        Listing_Standard listing = new Listing_Standard();
        listing.Begin(inRect);

        listing.Label($"Current Dolly Button: {ButtonName(Settings.MouseDollyButton)}");

        var buttonRect = listing.GetRect(30f);
        if (!waitingForClick)
        {
            if (Widgets.ButtonText(buttonRect, "Set dolly button"))
            {
                waitingForClick = true;
            }
        }
        else
        {
            Widgets.Label(buttonRect, "Click any mouse button...");
            if (waitingForClick && Event.current.type == EventType.MouseDown)
            {
                Settings.MouseDollyButton = Event.current.button;
                waitingForClick = false;
                Event.current.Use();
            }
        }

        var checkRect = listing.GetRect(30f);
        var disable = Settings.DisablePanningWhenColonistIsSelected;
        Widgets.CheckboxLabeled(checkRect, "Disable dollying when colonist is selected.", ref disable);
        Settings.DisablePanningWhenColonistIsSelected = disable;

        listing.End();
    }

}
