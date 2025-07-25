using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
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

        codes.MatchStartForward(
            [
                new CodeMatch(OpCodes.Ldc_I4_2),
                new CodeMatch(OpCodes.Call, mouseDragMethod),
                new CodeMatch(OpCodes.Brfalse_S),
                new CodeMatch(OpCodes.Call, isSteamDeckGetter),
                new CodeMatch(OpCodes.Brfalse_S)
            ]
        );

        if (!codes.IsValid)
        {
            Log.Error("[MouseDollyMapper] Could not find pattern in CameraDriverOnGUI.");
            return instructions;
        }

        
        var jumpTarget = codes.InstructionAt(2).operand;
        codes.RemoveInstructions(5);

        codes.Insert(
            [
                new CodeInstruction(OpCodes.Call, replacementMethod),
                new CodeInstruction(OpCodes.Brfalse_S, jumpTarget)
            ]
        );

        return codes.InstructionEnumeration();
    }
}
