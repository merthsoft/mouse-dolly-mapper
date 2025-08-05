using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using Verse;
using Verse.Steam;

namespace Merthsoft.MouseDollyMapper.Patches;

[HarmonyPatch(typeof(MainTabsRoot), nameof(MainTabsRoot.HandleLowPriorityShortcuts))]
public static class MainTabsRoot_HandleLowPriorityShortcuts
{
    public static bool Prefix(MainTabsRoot __instance)
    {
        if (MouseDollyMapper.MouseDollyButton != 1 || Event.current.button != 1)
            return true;

        return !MouseDollyMapper.SkipRightClickOnWorldMap(__instance);
    }

    //static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    //{
    //    var codes = new CodeMatcher(instructions);

    //    var getSelector = AccessTools.Method(typeof(Find), nameof(Find.Selector));
    //    var getNumSelected = AccessTools.PropertyGetter(typeof(Selector), nameof(Selector.NumSelected));
    //    var getEventCurrent = AccessTools.PropertyGetter(typeof(Event), nameof(Event.current));
    //    var getType = AccessTools.PropertyGetter(typeof(Event), nameof(Event.type));
    //    var getButton = AccessTools.PropertyGetter(typeof(Event), nameof(Event.button));
    //    var getWorldSelected = AccessTools.PropertyGetter(typeof(WorldRendererUtility), nameof(WorldRendererUtility.WorldSelected));
    //    var isSteamDeckGetter = AccessTools.PropertyGetter(typeof(SteamDeck), nameof(SteamDeck.IsSteamDeck));
    //    var getMouseButton = AccessTools.Method(typeof(Input), nameof(Input.GetMouseButton), [typeof(int)]);

    //    var replacementMethod = AccessTools.Method(typeof(MouseDollyMapper), nameof(MouseDollyMapper.RightClickOnWorldMap));

    //    CodeMatch[] matches =
    //    [
    //        new CodeMatch(OpCodes.Call, getSelector),
    //        new CodeMatch(OpCodes.Callvirt, getNumSelected),
    //        new CodeMatch(OpCodes.Brtrue_S),

    //        new CodeMatch(OpCodes.Call, getEventCurrent),
    //        new CodeMatch(OpCodes.Callvirt, getType),
    //        new CodeMatch(OpCodes.Brtrue_S),

    //        new CodeMatch(OpCodes.Call, getEventCurrent),
    //        new CodeMatch(OpCodes.Callvirt, getButton),
    //        new CodeMatch(OpCodes.Ldc_I4_1),
    //        new CodeMatch(OpCodes.Bne_Un_S),

    //        new CodeMatch(OpCodes.Call, getWorldSelected),
    //        new CodeMatch(OpCodes.Brtrue_S),

    //        new CodeMatch(OpCodes.Call, isSteamDeckGetter),
    //        new CodeMatch(OpCodes.Brfalse_S),
    //        new CodeMatch(OpCodes.Ldc_I4_2),
    //        new CodeMatch(OpCodes.Call, getMouseButton),
    //        new CodeMatch(OpCodes.Brtrue_S)
    //    ];

    //    codes.MatchStartForward(matches);

    //    if (!codes.IsValid)
    //    {
    //        Log.Error("[MouseDollyMapper] Could not find pattern in HandleLowPriorityShortcuts.");
    //        return instructions;
    //    }

    //    var jumpTarget = codes.InstructionAt(2).operand;
    //    codes.RemoveInstructions(matches.Length);

    //    codes.Insert([
    //        new CodeInstruction(OpCodes.Call, replacementMethod),
    //        new CodeInstruction(OpCodes.Brfalse_S, jumpTarget),
    //    ]);

    //    return codes.InstructionEnumeration();
    //}
}
