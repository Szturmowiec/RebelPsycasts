using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace IllegalPsycasts
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        private static readonly Type patchType = typeof(HarmonyPatches);
        static HarmonyPatches()
        {
            Harmony harmony = new Harmony(id: "illegal_psycasts_harmony");
            harmony.Patch(AccessTools.Method(typeof(Psycast), nameof(Psycast.Activate),
                new Type[] { typeof(LocalTargetInfo), typeof(LocalTargetInfo) }),
                postfix: new HarmonyMethod(patchType, nameof(notifyEmpireAboutIllegalPsycast)));

            harmony.Patch(AccessTools.Method(typeof(Psycast), nameof(Psycast.Activate), new Type[] { typeof(GlobalTargetInfo) }),
                postfix: new HarmonyMethod(patchType, nameof(notifyEmpireAboutIllegalPsycast)));

            harmony.Patch(AccessTools.Method(typeof(ThingRequiringRoyalPermissionUtility),
                nameof(ThingRequiringRoyalPermissionUtility.GetMinTitleToUse)),
                postfix: new HarmonyMethod(patchType, nameof(getMinTitleForImplantPatched)));

            harmony.Patch(AccessTools.Method(typeof(ThingRequiringRoyalPermissionUtility),
                nameof(ThingRequiringRoyalPermissionUtility.IsViolatingRulesOfAnyFaction)),
                postfix: new HarmonyMethod(patchType, nameof(IsViolatingRulesOfAnyFactionPatched)));

            harmony.Patch(AccessTools.Method(typeof(Verb_CastPsycast), nameof(Verb_CastPsycast.OnGUI)),
                prefix: new HarmonyMethod(patchType, nameof(displayDetectionChance)));
        }

        public static void notifyEmpireAboutIllegalPsycast(ref Psycast __instance)
        {
            Faction empire = Find.FactionManager.OfEmpire;
            if (PsycastDetectionutils.isPsycastIllegal(__instance.pawn, empire, __instance.def.level))
            {
                float detectionChance = PsycastDetectionutils.getDetectionChance(__instance.def);
                if (!Rand.Chance(detectionChance))
                {
                    return;
                }
                empire.Notify_RoyalThingUseViolation(__instance.def, __instance.pawn, __instance.def.defName, detectionChance, __instance.def.level);
            }
        }

        public static void IsViolatingRulesOfAnyFactionPatched(ref bool __result, Pawn pawn, int implantLevel)
        {
            foreach (Faction faction in Find.FactionManager.AllFactions)
            {
                __result = PsycastDetectionutils.isPsycastIllegal(pawn, faction, implantLevel);
                if (__result)
                {
                    return;
                }
            }
        }

        public static void getMinTitleForImplantPatched(ref RoyalTitleDef __result, int implantLevel)
        {
            List<RoyalTitleDef> availableTitles = Find.FactionManager.OfEmpire.def.RoyalTitlesAwardableInSeniorityOrderForReading;
            __result = availableTitles.Find(title => title.maxPsylinkLevel == implantLevel);
        }

        public static void displayDetectionChance(Verb_CastPsycast __instance)
        {
            __instance.Psycast.def.detectionChanceOverride = PsycastDetectionutils.getDetectionChance(__instance.Psycast.def);
        }
    }
}