﻿using AlienRace;

namespace MoyoMedicalExpansion
{
    public class GeneClass_RapidMetabolicBreakdown : Gene
    {
        /* Variables */
        private ModExtension_MoyoMedicalExpansion modExt;

        private RapidMetabolicBreakdown_Settings lockedRMBDSetting;

        internal int currentDosage;

        internal bool isTransformed;

        /* --- Properties --- */
        private ModExtension_MoyoMedicalExpansion ModExt
        {
            get
            {
                modExt ??= def.GetModExtension<ModExtension_MoyoMedicalExpansion>();
                return modExt;
            }
        }

        /* Fetches fields from the locked settings */
        private ThingDef LockedDrugThingDef => lockedRMBDSetting.drugThingDef;
        private int LockedDosagesToTransform => lockedRMBDSetting.dosageToTransform;
        private Color LockedTransformationColor => lockedRMBDSetting.transformationColor;
        private AbilityDef LockedAbilityGiven => lockedRMBDSetting.abilityGiven;


        public override void Notify_IngestedThing(Thing thing, int numTaken)
        {
            ThingDef drugThingDef = thing?.def;
            if (ModExt.RMBDSettings.NullOrEmpty() || isTransformed) return;


            // If it's taken the same drug as before
            if (lockedRMBDSetting is not null && drugThingDef == LockedDrugThingDef)
            {
                // If the current dosages don't meet our threshold we just add 1 to the counter
                ++currentDosage;

                if (currentDosage >= LockedDosagesToTransform)
                {
                    HediffClass_RapidMetabolicBreakdown hediff = (HediffClass_RapidMetabolicBreakdown)HediffMaker.MakeHediff(MoyoMedicalExpansion_HediffDefOfs.Thek_RapidMetabolicBreakDown, pawn);
                    hediff.bodyAddonColor = LockedTransformationColor;
                    hediff.abilityDefToGive = LockedAbilityGiven;
                    hediff.RMBDGene = this;

                    pawn.health.AddHediff(hediff);
                    currentDosage = 0;
                    isTransformed = true;

                    var alienComp = pawn.GetComp<AlienPartGenerator.AlienComp>();
                    alienComp.OverwriteColorChannel("RMBDColor", first: lockedRMBDSetting.transformationColor);
                    alienComp.RegenerateAddonsForced();
                }
            }
            else
            {
                // If it's taken a different drug, look for the options of the list and find the appropriate settings instance.
                for (int i = 0; i < ModExt.RMBDSettings.Count; i++)
                {
                    RapidMetabolicBreakdown_Settings setting = ModExt.RMBDSettings[i];
                    if (drugThingDef == setting.drugThingDef)
                    {
                        lockedRMBDSetting = setting;
                        currentDosage = 1; // dosages are resetted, but we're already taking a drug, so it's set to 1
                        return;
                    }
                }

                // If it's taken a different drug and it's not one of the options in the list, reset the dosages.
                currentDosage = 0;
            }
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref currentDosage, "Thek_RapidMetabolicBreakDown_currentDosage");
            Scribe_Values.Look(ref isTransformed, "Thek_RapidMetabolicBreakDown_isTranformed");
        }
    }
}
