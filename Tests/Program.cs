using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.IO;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Merthsoft.MouseDollyMapper;
using Merthsoft.MouseDollyMapper.Patches;
using UnityEngine;
using Verse;
using Verse.Steam;

static class Program
{
    static Vector2 position;
    static bool held;
    static WindowStack windows = new();
    static int assertions;
    static void Assert(bool value, string message)
    {
        if (!value) throw new Exception(message);
        assertions++;
    }
    static bool Position(ref Vector2 __result) { __result = position; return false; }
    static bool Held(ref bool __result) { __result = held; return false; }
    static bool NotDeck(ref bool __result) { __result = false; return false; }
    static bool Windows(ref WindowStack __result) { __result = windows; return false; }
    static void Patch(Harmony h, MethodBase method, string prefix)
        => h.Patch(method, prefix: new HarmonyMethod(typeof(Program).GetMethod(prefix, BindingFlags.Static | BindingFlags.NonPublic)));

    static void Main()
    {
        var h = new Harmony("MouseDollyMapper.Regression");
        Patch(h, AccessTools.PropertyGetter(typeof(UI), nameof(UI.MousePositionOnUIInverted)), nameof(Position));
        Patch(h, AccessTools.Method(typeof(Input), nameof(Input.GetMouseButton), new[] {typeof(int)}), nameof(Held));
        Patch(h, AccessTools.Method(typeof(Input), nameof(Input.GetKey), new[] {typeof(KeyCode)}), nameof(Held));
        Patch(h, AccessTools.PropertyGetter(typeof(SteamDeck), nameof(SteamDeck.IsSteamDeck)), nameof(NotDeck));
        Patch(h, AccessTools.PropertyGetter(typeof(Find), nameof(Find.WindowStack)), nameof(Windows));
        Patch(h, AccessTools.PropertyGetter(typeof(Prefs), nameof(Prefs.LogVerbose)), nameof(NotDeck));
        var settings = new MouseDollyMapperSettings { DisablePanningWhenColonistIsSelected = false };
        AccessTools.Field(typeof(MouseDollyMapper), "Settings").SetValue(null, settings);
        Assert(settings.DollyKey == KeyCode.None && settings.MouseDollyButton == 2 && !settings.DisableDollying,
            "New defaults must preserve middle mouse and enable dollying");
        settings.MouseDollyButton = 1;
        Assert(MouseDollyMapper.UsesRightMouse, "Legacy right mouse binding preserved");
        settings.DollyKey = KeyCode.F11;
        Assert(!MouseDollyMapper.UsesRightMouse, "Keyboard binding must not hijack right click");
        var tracker = new MouseButtonTracker(null);
        held = true;
        position = new Vector2(100,100);
        tracker.GameComponentUpdate();
        Assert(!MouseDollyMapper.DollyButtonPressed(), "Press must not jump from stale pointer origin");
        position = new Vector2(110,120);
        Assert(MouseDollyMapper.DollyButtonPressed(), "Held keyboard plus pointer movement must pan without MouseDrag");
        Assert(MouseButtonTracker.DollyDelta() == new Vector2(10,20), "Pointer displacement and sign");
        Assert(!MouseDollyMapper.DollyButtonPressed(), "Repeated GUI call must not double-consume displacement");
        Assert(MouseDollyMapper.HideMainUiButtons(), "Dragging hides UI when enabled");
        settings.DisableDollying = true;
        Assert(!MouseDollyMapper.DollyButtonPressed() && !MouseDollyMapper.HideMainUiButtons() && !MouseDollyMapper.HideOpenTab(),
            "Global disable stops movement and UI hiding immediately");
        Assert(MainTabsRoot_HandleLowPriorityShortcuts.Prefix(null), "Disabled dollying must preserve vanilla shortcuts");
        tracker.GameComponentUpdate();
        settings.DisableDollying = false;
        tracker.GameComponentUpdate();
        Assert(!MouseDollyMapper.DollyButtonPressed(), "Reenable must initialize fresh pointer origin");
        held = false;
        tracker.GameComponentUpdate();
        Assert(!MouseButtonTracker.HasDragged(), "Release restores UI");
        settings.DragDelayMs = 120;
        held = true;
        tracker.GameComponentUpdate();
        position = new Vector2(130,140);
        Assert(!MouseDollyMapper.DollyButtonPressed(), "Delay blocks early displacement");
        var elapsed = Stopwatch.StartNew();
        Thread.Sleep(160); // No TickManager exists: simulation cannot advance.
        position = new Vector2(140,150);
        Assert(MouseDollyMapper.DollyButtonPressed(), "Real-time delay expires without simulation ticks");
        Assert(MouseButtonTracker.DollyDelta() == new Vector2(10,10), "Delay does not accumulate pre-delay movement");
        var timing = AccessTools.Method(typeof(MouseButtonTracker), "DelayPassed");
        Assert(!(bool)timing.Invoke(null, new object[] {499.9,500}), "Delay does not expire before exact threshold");
        Assert((bool)timing.Invoke(null, new object[] {500.0,500}), "Delay expires at threshold");
        Assert((bool)timing.Invoke(null, new object[] {0.0,0}), "Zero delay retains immediate default");        var canBind = AccessTools.Method(typeof(MouseDollyMapper), "CanBindKey");
        Assert(!(bool)canBind.Invoke(null, new object[] {KeyCode.LeftShift, EventModifiers.None}), "Reject standalone modifier");
        Assert(!(bool)canBind.Invoke(null, new object[] {KeyCode.F11, EventModifiers.Control}), "Reject modifier combination");
        Assert((bool)canBind.Invoke(null, new object[] {KeyCode.F11, EventModifiers.CapsLock}), "Caps lock is not a binding modifier");
        var keyDef = new KeyBindingDef {defName = "RegressionKey", label = "Regression key"};
        DefDatabase<KeyBindingDef>.Add(keyDef);
        KeyPrefs.KeyPrefsData = new KeyPrefsData();
        KeyPrefs.KeyPrefsData.keyPrefs[keyDef] = new KeyBindingData(KeyCode.F10, KeyCode.F12);
        var conflictMethod = AccessTools.Method(typeof(MouseDollyMapper), "BindingConflict");
        Assert(conflictMethod.Invoke(null, new object[] {KeyCode.F10}) == keyDef, "Detect primary key conflict");
        Assert(conflictMethod.Invoke(null, new object[] {KeyCode.F12}) == keyDef, "Detect secondary key conflict");
        Assert(conflictMethod.Invoke(null, new object[] {KeyCode.F11}) == null, "Allow unbound key");
        var mod = (MouseDollyMapper)RuntimeHelpers.GetUninitializedObject(typeof(MouseDollyMapper));
        var capture = AccessTools.Method(typeof(MouseDollyMapper), "CaptureBinding");
        var capturing = AccessTools.Field(typeof(MouseDollyMapper), "CapturingBinding");
        capturing.SetValue(null, true);
        capture.Invoke(mod, new object[] {new Event {type=EventType.KeyDown, keyCode=KeyCode.Escape}});
        Assert(!(bool)capturing.GetValue(null) && settings.DollyKey == KeyCode.F11, "Escape cancels capture without changing binding");
        capturing.SetValue(null, true);
        capture.Invoke(mod, new object[] {new Event {type=EventType.MouseDown, button=4}});
        Assert(settings.DollyKey == KeyCode.None && settings.MouseDollyButton == 4, "Mouse capture switches back and preserves mouse binding");
        capturing.SetValue(null, true);
        capture.Invoke(mod, new object[] {new Event {type=EventType.KeyDown, keyCode=KeyCode.F11}});
        Assert(settings.DollyKey == KeyCode.F11 && settings.MouseDollyButton == 4 && !(bool)capturing.GetValue(null), "Key capture retains old mouse index");
        settings.DragDelayMs = 0;
        position = new Vector2(500,500);
        MouseButtonTracker.FinishCameraEvent();
        position = new Vector2(510,510);
        Assert(MouseDollyMapper.DollyButtonPressed() && MouseButtonTracker.DollyDelta() == new Vector2(10,10), "No accumulated jump after camera-blocking UI");
        var settingsFile = Path.Combine(Path.GetTempPath(), "mouse-dolly-regression-" + Guid.NewGuid() + ".xml");
        try
        {
            File.WriteAllText(settingsFile, "<save><MouseDollyButton>4</MouseDollyButton><DragDelayMs>500</DragDelayMs></save>");
            var migrated = new MouseDollyMapperSettings();
            Scribe.loader.InitLoading(settingsFile);
            migrated.ExposeData();
            Scribe.loader.FinalizeLoading();
            Assert(migrated.MouseDollyButton == 4 && migrated.DollyKey == KeyCode.None && !migrated.DisableDollying && migrated.DragDelayMs == 500,
                "Legacy settings load without new fields and preserve mouse/delay");
            settings.DisableDollying = true;
            Scribe.saver.InitSaving(settingsFile, "save");
            settings.ExposeData();
            Scribe.saver.FinalizeSaving();
            var roundTrip = new MouseDollyMapperSettings();
            Scribe.loader.InitLoading(settingsFile);
            roundTrip.ExposeData();
            Scribe.loader.FinalizeLoading();
            Assert(roundTrip.DollyKey == KeyCode.F11 && roundTrip.MouseDollyButton == 4 && roundTrip.DisableDollying,
                "Keyboard/disable settings persist with original mouse index");
        }
        finally { File.Delete(settingsFile); }
        var original = PatchProcessor.GetOriginalInstructions(AccessTools.Method(typeof(CameraDriver), nameof(CameraDriver.CameraDriverOnGUI))).ToList();
        var transpiler = AccessTools.Method(typeof(CameraDriver_CameraDriverOnGUI), "Transpiler");
        var patched = ((IEnumerable<CodeInstruction>)transpiler.Invoke(null, new object[] {original})).ToList();
        Assert(patched.Count(i => i.Calls(AccessTools.Method(typeof(MouseDollyMapper), nameof(MouseDollyMapper.DollyButtonPressed)))) == 1,
            "Installed game camera guard must match transpiler");
        Assert(patched.Count(i => i.Calls(AccessTools.Method(typeof(MouseButtonTracker), nameof(MouseButtonTracker.DollyDelta)))) == 1,
            "Installed game camera delta must match transpiler");
        Assert(!patched.Any(i => i.Calls(AccessTools.PropertyGetter(typeof(UnityGUIBugsFixer), nameof(UnityGUIBugsFixer.CurrentEventDelta)))),
            "Camera must use sampled displacement instead of event delta");
        var labels = patched.SelectMany(i => i.labels).ToHashSet();
        Assert(patched.Where(i => i.operand is Label).All(i => labels.Contains((Label)i.operand)), "All branch targets retained");
        Console.WriteLine($"PASS: {assertions} assertions; real delay elapsed {elapsed.ElapsedMilliseconds} ms; camera transpiler matches installed assembly.");
        h.UnpatchAll(h.Id);
    }
}
