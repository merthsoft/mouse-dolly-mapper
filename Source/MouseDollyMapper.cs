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

    public override string SettingsCategory()
        => "Merthsoft.MouseDollyMapper.SettingsCategory".Translate();

    private string ButtonName(int i) => i switch
    {
        0 => "Merthsoft.MouseDollyMapper.ButtonLeft".Translate(),
        1 => "Merthsoft.MouseDollyMapper.ButtonRight".Translate(),
        2 => "Merthsoft.MouseDollyMapper.ButtonMiddle".Translate(),
        3 => "Merthsoft.MouseDollyMapper.ButtonBack".Translate(),
        4 => "Merthsoft.MouseDollyMapper.ButtonForward".Translate(),
        _ => "Merthsoft.MouseDollyMapper.ButtonGeneric".Translate(i),
    };

    private bool waitingForClick = false;
    public override void DoSettingsWindowContents(Rect inRect)
    {
        var listing = new Listing_Standard();
        listing.Begin(inRect);

        listing.Label("Merthsoft.MouseDollyMapper.CurrentDollyButton".Translate(ButtonName(Settings.MouseDollyButton)));

        var buttonRect = listing.GetRect(30f);
        if (!waitingForClick)
        {
            if (Widgets.ButtonText(buttonRect, "Merthsoft.MouseDollyMapper.SetDollyButton".Translate()))
            {
                waitingForClick = true;
            }
        }
        else
        {
            Widgets.Label(buttonRect, "Merthsoft.MouseDollyMapper.ClickAnyMouseButton".Translate());
            if (waitingForClick && Event.current.type == EventType.MouseDown)
            {
                Settings.MouseDollyButton = Event.current.button;
                waitingForClick = false;
                Event.current.Use();
            }
        }

        var checkRect = listing.GetRect(30f);
        var disable = Settings.DisablePanningWhenColonistIsSelected;
        Widgets.CheckboxLabeled(checkRect, "Merthsoft.MouseDollyMapper.DisableWhenColonistSelected".Translate(), ref disable);
        Settings.DisablePanningWhenColonistIsSelected = disable;

        listing.End();
    }
}
