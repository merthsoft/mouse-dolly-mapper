using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using Verse;
using Verse.Steam;

namespace Merthsoft.MouseDollyMapper.Patches;

[HarmonyPatch(typeof(CameraDriver), nameof(CameraDriver.CameraDriverOnGUI))]
public static class CameraDriver_CameraDriverOnGUI
{
    static void Postfix() => MouseButtonTracker.FinishCameraEvent();

    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new CodeMatcher(instructions);

        var mouseDragMethod = AccessTools.Method(typeof(UnityGUIBugsFixer), nameof(UnityGUIBugsFixer.MouseDrag));
        var isSteamDeckGetter = AccessTools.PropertyGetter(typeof(SteamDeck), nameof(SteamDeck.IsSteamDeck));
        var replacementMethod = AccessTools.Method(typeof(MouseDollyMapper), nameof(MouseDollyMapper.DollyButtonPressed));
        var getSelector = AccessTools.PropertyGetter(typeof(Find), nameof(Find.Selector));
        var getAnyPawnSelector = AccessTools.PropertyGetter(typeof(Selector), nameof(Selector.AnyPawnSelected));

        CodeMatch[] matches = [
            new CodeMatch(OpCodes.Ldc_I4_2),
            new CodeMatch(OpCodes.Call, mouseDragMethod),
            new CodeMatch(OpCodes.Brfalse_S),
            new CodeMatch(OpCodes.Call, isSteamDeckGetter),
            new CodeMatch(OpCodes.Brfalse_S),
            new CodeMatch(OpCodes.Call, getSelector),
            new CodeMatch(OpCodes.Callvirt, getAnyPawnSelector),
            new CodeMatch(OpCodes.Brtrue_S),
        ];

        codes.MatchStartForward(matches);

        if (!codes.IsValid)
        {
            Log.Error("Could not find pattern in CameraDriverOnGUI.");
            return instructions;
        }
        
        var jumpTarget = codes.InstructionAt(2).operand;
        codes.RemoveInstructions(matches.Length);

        codes.Insert(
            [
                new CodeInstruction(OpCodes.Call, replacementMethod),
                new CodeInstruction(OpCodes.Brfalse_S, jumpTarget)
            ]
        );

        // Keyboard-held panning must use pointer displacement even on non-MouseDrag GUI events.
        codes.Start().MatchStartForward(new CodeMatch(OpCodes.Call,
            AccessTools.PropertyGetter(typeof(UnityGUIBugsFixer), nameof(UnityGUIBugsFixer.CurrentEventDelta))));
        if (!codes.IsValid)
        {
            Log.Error("Could not find camera dolly delta in CameraDriverOnGUI.");
            return instructions;
        }
        codes.SetOperandAndAdvance(AccessTools.Method(typeof(MouseButtonTracker), nameof(MouseButtonTracker.DollyDelta)));
        return codes.InstructionEnumeration();
    }
}
