using RimWorld;
using System.Linq;
using Verse;

namespace IllegalPsycasts
{
    public static class PsycastDetectionutils
    {
        static PsycastDetectionutils()
        {

        }

        public static bool isPsycastIllegal(Pawn pawn, Faction faction, int implantLevel)
        {
            if (!Find.FactionManager.OfEmpire.Equals(faction) || hasSilencer(pawn))
            {
                return false;
            }
            RoyalTitleDef title = pawn.royalty.GetCurrentTitle(faction);
            return title == null || implantLevel > title.index;
        }

        public static float getDetectionChance(AbilityDef psycast)
        {
            if (psycast.EntropyGain == 0)
            {
                return (float) psycast.level / 10;
            }
            return psycast.EntropyGain / 100;
        }

        private static bool hasSilencer(Pawn pawn)
        {
            return pawn.health.hediffSet.hediffs.Any(implant => implant.def.defName.Equals("HediffPsychicSilencer"));
        }
    }
}
