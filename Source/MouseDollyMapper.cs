using HarmonyLib;
using Merthsoft.MouseDollyMapper.Enums;
using Merthsoft.MouseDollyMapper.Patches;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Reflection;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Steam;

namespace Merthsoft.MouseDollyMapper;

public class MouseDollyMapper : Mod
{
    private static MouseDollyMapperSettings Settings;

    public static int MouseDollyButton => Settings.MouseDollyButton; 
    public static int DragDelayMs => Settings.DragDelayMs;
    private static bool CapturingBinding;
    public static bool UsesRightMouse => Settings.DollyKey == KeyCode.None && MouseDollyButton == 1;
    public static bool CanDolly => !Settings.DisableDollying && !CapturingBinding
        && !SteamDeck.IsSteamDeck
        && !(Settings.DisablePanningWhenColonistIsSelected && Find.Selector.AnyPawnSelected);

    public static bool BindingHeld()
    {
        try
        {
            return Settings.DollyKey == KeyCode.None
                ? Input.GetMouseButton(MouseDollyButton)
                : GUIUtility.keyboardControl == 0 && !Find.WindowStack.AnySearchWidgetFocused && Input.GetKey(Settings.DollyKey);
        }
        catch { return false; }
    }

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
        if (!CanDolly || !UsesRightMouse || Event.current.button != 1)
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
        => CanDolly && MouseButtonTracker.IsMouseDragged();

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

    private string bindingError;
    public override void DoSettingsWindowContents(Rect inRect)
    {
        var listing = new Listing_Standard();
        listing.Begin(inRect);
        var disabledBefore = Settings.DisableDollying;
        listing.CheckboxLabeled("Merthsoft.MouseDollyMapper.DisableDollying".Translate(), ref Settings.DisableDollying);
        if (disabledBefore != Settings.DisableDollying)
            MouseButtonTracker.Reset();

        var bindingName = Settings.DollyKey == KeyCode.None ? ButtonName(Settings.MouseDollyButton) : Settings.DollyKey.ToString();
        listing.Label("Merthsoft.MouseDollyMapper.CurrentDollyBinding".Translate(bindingName));
        if (!CapturingBinding)
        {
            if (listing.ButtonText("Merthsoft.MouseDollyMapper.SetDollyBinding".Translate()))
            {
                CapturingBinding = true;
                bindingError = null;
                MouseButtonTracker.Reset();
            }
        }
        else
        {
            listing.Label("Merthsoft.MouseDollyMapper.CaptureBinding".Translate());
            CaptureBinding(Event.current);
        }
        if (bindingError != null)
            listing.Label(bindingError);

        if (UsesRightMouse && !Settings.DisableDollying)
        {
            listing.Label("Merthsoft.MouseDollyMapper.ArchitectMenuModeLabel".Translate());
            foreach (ArchitectMenuMode mode in Enum.GetValues(typeof(ArchitectMenuMode)))
                if (listing.RadioButton("\t" + ArchitectMenuModeString(mode), Settings.ArchitectMenuMode == mode))
                    Settings.ArchitectMenuMode = mode;
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

    private void CaptureBinding(Event ev)
    {
        if (ev.type == EventType.KeyDown && ev.keyCode == KeyCode.Escape)
        {
            CapturingBinding = false;
            bindingError = null;
            ev.Use();
        }
        else if (ev.type == EventType.MouseDown)
        {
            Settings.MouseDollyButton = ev.button;
            Settings.DollyKey = KeyCode.None;
            CapturingBinding = false;
            bindingError = null;
            ev.Use();
        }
        else if (ev.type == EventType.KeyDown && ev.keyCode != KeyCode.None)
        {
            var key = ev.keyCode;
            var conflict = BindingConflict(key);
            if (!CanBindKey(key, ev.modifiers) || conflict != null)
                bindingError = "Merthsoft.MouseDollyMapper.BindingConflict".Translate(conflict?.LabelCap.ToString() ?? key.ToString());
            else
            {
                Settings.DollyKey = key;
                CapturingBinding = false;
                bindingError = null;
            }
            ev.Use();
        }
    }

    internal static bool CanBindKey(KeyCode key, EventModifiers modifiers)
        => key != KeyCode.None && key != KeyCode.Escape
        && (modifiers & (EventModifiers.Shift | EventModifiers.Control | EventModifiers.Alt | EventModifiers.Command)) == 0
        && key is not (KeyCode.LeftShift or KeyCode.RightShift or KeyCode.LeftControl or KeyCode.RightControl
            or KeyCode.LeftAlt or KeyCode.RightAlt or KeyCode.LeftCommand or KeyCode.RightCommand);

    internal static KeyBindingDef BindingConflict(KeyCode key)
        => DefDatabase<KeyBindingDef>.AllDefsListForReading.FirstOrDefault(def =>
            KeyPrefs.KeyPrefsData.GetBoundKeyCode(def, KeyPrefs.BindingSlot.A) == key ||
            KeyPrefs.KeyPrefsData.GetBoundKeyCode(def, KeyPrefs.BindingSlot.B) == key);
    public override void WriteSettings()
    {
        CapturingBinding = false;
        MouseButtonTracker.Reset();
        base.WriteSettings();
    }

    private static string ArchitectMenuModeString(ArchitectMenuMode mode)
        => $"Merthsoft.MouseDollyMapper.ArchitectMenuMode.{mode}".Translate();

    private static string UiHidingModeString(UiHidingMode mode)
        => $"Merthsoft.MouseDollyMapper.UiHidingMode.{mode}".Translate();
}
