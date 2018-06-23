using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace EdB.PrepareCarefully {
    public class ProviderBodyTypes {
        protected Dictionary<ThingDef, OptionsBodyType> raceBodyTypeLookup = new Dictionary<ThingDef, OptionsBodyType>();
        protected Dictionary<BodyTypeDef, string> bodyTypeLabels = new Dictionary<BodyTypeDef, string>();
        public ProviderBodyTypes() {
            bodyTypeLabels.Add(BodyTypeDefOf.Fat, "EdB.PC.Pawn.BodyTypeDefOf.Fat".Translate());
            bodyTypeLabels.Add(BodyTypeDefOf.Hulk, "EdB.PC.Pawn.BodyTypeDefOf.Hulk".Translate());
            bodyTypeLabels.Add(BodyTypeDefOf.Thin, "EdB.PC.Pawn.BodyTypeDefOf.Thin".Translate());
            bodyTypeLabels.Add(BodyTypeDefOf.Male, "EdB.PC.Pawn.BodyTypeDefOf.Average".Translate());
            bodyTypeLabels.Add(BodyTypeDefOf.Female, "EdB.PC.Pawn.BodyTypeDefOf.Average".Translate());
        }
        public ProviderAlienRaces AlienRaceProvider {
            get; set;
        }
        public List<BodyTypeDef> GetBodyTypesForPawn(CustomPawn pawn) {
            return GetBodyTypesForPawn(pawn.Pawn);
        }
        public List<BodyTypeDef> GetBodyTypesForPawn(Pawn pawn) {
            OptionsBodyType bodyTypes;
            if (!raceBodyTypeLookup.TryGetValue(pawn.def, out bodyTypes)) {
                bodyTypes = InitializeBodyTypes(pawn.def);
                raceBodyTypeLookup.Add(pawn.def, bodyTypes);
            }
            return bodyTypes.GetBodyTypes(pawn.gender);
        }
        public string GetBodyTypeLabel(BodyTypeDef bodyType) {
            return bodyTypeLabels[bodyType];
        }
        protected OptionsBodyType InitializeBodyTypes(ThingDef def) {
            if (!ProviderAlienRaces.IsAlienRace(def)) {
                return InitializeHumanlikeBodyTypes();
            }
            else {
                OptionsBodyType result = InitializeAlienRaceBodyTypes(def);
                if (result == null) {
                    Log.Warning("Prepare Carefully could not initialize body types for alien race, " + def.defName + ". Defaulting to humanlike body types.");
                    return InitializeHumanlikeBodyTypes();
                }
                if (result.MaleBodyTypes.Count == 0 || result.FemaleBodyTypes.Count == 0) {
                    return InitializeHumanlikeBodyTypes();
                }
                else {
                    return result;
                }
            }
        }
        protected OptionsBodyType InitializeHumanlikeBodyTypes() {
            OptionsBodyType result = new OptionsBodyType();
            result.MaleBodyTypes.Add(BodyTypeDefOf.Male);
            result.MaleBodyTypes.Add(BodyTypeDefOf.Thin);
            result.MaleBodyTypes.Add(BodyTypeDefOf.Fat);
            result.MaleBodyTypes.Add(BodyTypeDefOf.Hulk);
            result.FemaleBodyTypes.Add(BodyTypeDefOf.Female);
            result.FemaleBodyTypes.Add(BodyTypeDefOf.Thin);
            result.FemaleBodyTypes.Add(BodyTypeDefOf.Fat);
            result.FemaleBodyTypes.Add(BodyTypeDefOf.Hulk);
            result.NoGenderBodyTypes.Add(BodyTypeDefOf.Male);
            result.NoGenderBodyTypes.Add(BodyTypeDefOf.Thin);
            result.NoGenderBodyTypes.Add(BodyTypeDefOf.Fat);
            result.NoGenderBodyTypes.Add(BodyTypeDefOf.Hulk);
            return result;
        }
        protected OptionsBodyType InitializeAlienRaceBodyTypes(ThingDef def) {
            OptionsBodyType result = new OptionsBodyType();
            AlienRace alienRace = AlienRaceProvider.GetAlienRace(def);
            if (alienRace == null) {
                return null;
            }
            if (alienRace.BodyTypes.Count > 0) {
                bool containsMale = alienRace.BodyTypes.Contains(BodyTypeDefOf.Male);
                bool containsFemale = alienRace.BodyTypes.Contains(BodyTypeDefOf.Female);
                bool containsBothMaleAndFemale = containsMale && containsFemale;
                foreach (BodyTypeDef type in alienRace.BodyTypes) {
                    if (type != BodyTypeDefOf.Male || !containsBothMaleAndFemale) {
                        result.FemaleBodyTypes.Add(type);
                    }
                    if (type != BodyTypeDefOf.Female || !containsBothMaleAndFemale) {
                        result.MaleBodyTypes.Add(type);
                    }
                    result.NoGenderBodyTypes.Add(type);
                }
            }

            if (result.MaleBodyTypes.Count == 0 && result.FemaleBodyTypes.Count > 0) {
                result.MaleBodyTypes = result.FemaleBodyTypes;
            }
            else if (result.FemaleBodyTypes.Count == 0 && result.MaleBodyTypes.Count > 0) {
                result.FemaleBodyTypes = result.MaleBodyTypes;
            }

            return result;
        }
    }
}