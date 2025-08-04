using HarmonyLib;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Steam;

namespace Merthsoft.MouseDollyMapper;

public class MouseDollyMapper : Mod
{
    private static MouseDollyMapperSettings Settings;

    public static int MouseDollyButton => Settings.MouseDollyButton;
    public static bool DisableArchitectMenuOnRightMouse => Settings.DisableArchitectMenuOnRightMouse;

    public static bool HideMainUiButtons()
        => Settings.HideMainUiButtonsWhenDragging
        && MouseButtonTracker.HasDragged();

    public static bool RightClickOnWorldMap()
        => Find.Selector.NumSelected == 0
        && Event.current.type == EventType.MouseDown
        && Event.current.button == 1
        && !WorldRendererUtility.WorldSelected
        && (!SteamDeck.IsSteamDeck || !Input.GetMouseButton(2))
        && (MouseDollyButton != 1 || !Settings.DisableArchitectMenuOnRightMouse);

    public static bool DollyButtonPressed()
        => !SteamDeck.IsSteamDeck
        && MouseButtonTracker.IsMouseDragged()
        && !(Settings.DisablePanningWhenColonistIsSelected && Find.Selector.AnyPawnSelected);

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

        if (Settings.MouseDollyButton == 1)
        {
            var disableArchitectCheckRect = listing.GetRect(30f);
            Widgets.CheckboxLabeled(
                rect: disableArchitectCheckRect,
                label: "Merthsoft.MouseDollyMapper.DisableArchitectOnRightMouse".Translate(),
                checkOn: ref Settings.DisableArchitectMenuOnRightMouse);
        }

        var checkRect = listing.GetRect(30f);
        Widgets.CheckboxLabeled(
            rect: checkRect, 
            label: "Merthsoft.MouseDollyMapper.DisableWhenColonistSelected".Translate(), 
            checkOn: ref Settings.DisablePanningWhenColonistIsSelected);

        var hideMainUiWhenDraggingRect = listing.GetRect(30f);
        Widgets.CheckboxLabeled(
            rect: hideMainUiWhenDraggingRect,
            label: "Merthsoft.MouseDollyMapper.HideMainUiButtonsWhenDragging".Translate(),
            checkOn: ref Settings.HideMainUiButtonsWhenDragging);

        listing.End();
    }
}
