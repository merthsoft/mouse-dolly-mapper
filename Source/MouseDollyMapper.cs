using HarmonyLib;
using Merthsoft.MouseDollyMapper.Enums;
using Merthsoft.MouseDollyMapper.Patches;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Reflection;
using System.Runtime;
using UnityEngine;
using Verse;
using Verse.Steam;

namespace Merthsoft.MouseDollyMapper;

public class MouseDollyMapper : Mod
{
    private static MouseDollyMapperSettings Settings;

    public static int MouseDollyButton => Settings.MouseDollyButton; 
    public static int DragDelayTicks => (int)(Settings.DragDelayMs / 1000.0 * 60.0);

    public static bool HideMainUiButtons()
        => Settings.UiHidingMode == UiHidingMode.AllUi
        && MouseButtonTracker.HasDragged();

    public static bool HideOpenTab()
        => Settings.UiHidingMode == UiHidingMode.OpenTab
        && MouseButtonTracker.HasDragged();

    private static bool MouseUpArchitectMenuCheck()
        => Find.Selector.NumSelected == 0
        && Event.current.type == EventType.MouseUp
        && !MouseButtonTracker.HasDragged()
        && Event.current.button == 1
        && !WorldRendererUtility.WorldSelected
        && (!SteamDeck.IsSteamDeck || !Input.GetMouseButton(2));

    public static bool SkipRightClickOnWorldMap(MainTabsRoot mainTabsRoot)
    {
        if (!(MouseDollyButton == 1 && Event.current.button == 1))
            return false;

        switch (Settings.ArchitectMenuMode)
        {
            case ArchitectMenuMode.Vanilla:
                return false;
            case ArchitectMenuMode.Disable:
                return true;
            default:
                HandleLowPriorityShortcuts(mainTabsRoot);
                return true;
        }
    }

    private static MethodInfo AutoCloseInspectionTabIfNothingSelectedMethod
        = AccessTools.Method(typeof(MainTabsRoot), "AutoCloseInspectionTabIfNothingSelected");

    private static void HandleLowPriorityShortcuts(MainTabsRoot mainTabsRoot)
    {
        if (MouseButtonTracker.HasDragged())
            return;

        AutoCloseInspectionTabIfNothingSelectedMethod.Invoke(mainTabsRoot, [true]);
        
        if (Find.Selector.NumSelected == 0 && Event.current.type == EventType.MouseUp && Event.current.button == 1 && !WorldRendererUtility.WorldSelected && (!SteamDeck.IsSteamDeck || !Input.GetMouseButton(2)))
        {
            Event.current.Use();
            MainButtonDefOf.Architect.Worker.InterfaceTryActivate();
        }
        
        if (mainTabsRoot.OpenTab != null && mainTabsRoot.OpenTab != MainButtonDefOf.Inspect && Event.current.type == EventType.MouseUp && Event.current.button != 2)
        {
            mainTabsRoot.EscapeCurrentTab(true);
            if (Event.current.button == 0)
            {
                Find.Selector.ClearSelection();
                Find.WorldSelector.ClearSelection();
            }
        }
    }

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

        if (!waitingForClick)
        {
            if (listing.ButtonText("Merthsoft.MouseDollyMapper.SetDollyButton".Translate()))
            {
                waitingForClick = true;
            }
        }
        else
        {
            listing.Label("Merthsoft.MouseDollyMapper.ClickAnyMouseButton".Translate());
            if (waitingForClick && Event.current.type == EventType.MouseDown)
            {
                Settings.MouseDollyButton = Event.current.button;
                waitingForClick = false;
                Event.current.Use();
            }
        }

        if (Settings.MouseDollyButton == 1)
        {
            listing.Label("Merthsoft.MouseDollyMapper.ArchitectMenuModeLabel".Translate());

            foreach (ArchitectMenuMode mode in Enum.GetValues(typeof(ArchitectMenuMode)))
            {
                var isSelected = (Settings.ArchitectMenuMode == mode);
                if (listing.RadioButton("\t" + ArchitectMenuModeString(mode), isSelected))
                {
                    Settings.ArchitectMenuMode = mode;
                }
            }
        }

        listing.GapLine();

        listing.CheckboxLabeled(
            label: "Merthsoft.MouseDollyMapper.DisableWhenColonistSelected".Translate(), 
            checkOn: ref Settings.DisablePanningWhenColonistIsSelected);

        listing.Label("Merthsoft.MouseDollyMapper.DragDetectionDelay".Translate(Settings.DragDelayMs));
        Settings.DragDelayMs = (int)listing.Slider(Settings.DragDelayMs, 0, 1000);

        listing.GapLine();

        listing.Label("Merthsoft.MouseDollyMapper.UiHidingModeLabel".Translate());

        foreach (UiHidingMode mode in Enum.GetValues(typeof(UiHidingMode)))
        {
            if (mode == UiHidingMode.ArchitectMenu)
                continue;

            var isSelected = (Settings.UiHidingMode == mode);
            if (listing.RadioButton("\t" + UiHidingModeString(mode), isSelected))
            {
                Settings.UiHidingMode = mode;
            }
        }

        listing.End();
    }

    private static string ArchitectMenuModeString(ArchitectMenuMode mode)
        => $"Merthsoft.MouseDollyMapper.ArchitectMenuMode.{mode}".Translate();

    private static string UiHidingModeString(UiHidingMode mode)
        => $"Merthsoft.MouseDollyMapper.UiHidingMode.{mode}".Translate();
}
