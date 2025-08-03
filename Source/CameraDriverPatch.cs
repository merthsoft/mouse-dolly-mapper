using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using Verse;
using Verse.Steam;

namespace Merthsoft.MouseDollyMapper;

[HarmonyPatch(typeof(CameraDriver), nameof(CameraDriver.CameraDriverOnGUI))]
public static class CameraDriverPatch
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new CodeMatcher(instructions);

        var mouseDragMethod = AccessTools.Method(typeof(UnityGUIBugsFixer), nameof(UnityGUIBugsFixer.MouseDrag));
        var isSteamDeckGetter = AccessTools.PropertyGetter(typeof(SteamDeck), nameof(SteamDeck.IsSteamDeck));
        var replacementMethod = AccessTools.Method(typeof(MouseDollyMapper), nameof(MouseDollyMapper.DollyButtonPressed));
        var getSelector = AccessTools.Method(typeof(Find), nameof(Find.Selector));
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
            Log.Error("[MouseDollyMapper] Could not find pattern in CameraDriverOnGUI.");
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

        return codes.InstructionEnumeration();
    }
}
