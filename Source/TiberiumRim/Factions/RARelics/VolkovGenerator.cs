using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public static class VolkovGenerator
    {
        private static Pawn Volkov;

        //TODO: Reimplement
        /*
        public static Pawn GenerateVolkovCyborg(Map map)
        {
            Pawn volkov = GenerateVolkov(map);
            var parts = volkov.def.race.body.AllParts;
            volkov.health.AddHediff(DefDatabase<HediffDef>.GetNamed("RegenerativeNanites"));
            volkov.health.AddHediff(DefDatabase<HediffDef>.GetNamed("AugmentedEye"), parts.FirstOrDefault(p => p.IsInGroup(BodyPartGroupDefOf.Eyes)));
            volkov.health.AddHediff(DefDatabase<HediffDef>.GetNamed("CannonImplant"), parts.FirstOrDefault(p => p.def == BodyPartDefOf.Arm));
            return volkov;
        }

        public static Pawn GenerateVolkov(Map map)
        {
            Pawn pawn = (Pawn) ThingMaker.MakeThing(RedAlertDefOf.Volkov.race);
            pawn.kindDef = RedAlertDefOf.Volkov;
            pawn.SetFactionDirect(Find.FactionManager.FirstFactionOfDef(RedAlertDefOf.SovjetFaction));
            PawnComponentsUtility.CreateInitialComponents(pawn);

            pawn.gender = Gender.Male;
            
            //Age
            pawn.ageTracker.AgeBiologicalTicks = 30 * GenDate.TicksPerYear;
            pawn.ageTracker.AgeChronologicalTicks = (GenDate.Year(Find.TickManager.TicksAbs, Find.WorldGrid.LongLatOf(map.Tile).x) - 1948) * GenDate.TicksPerYear;
            pawn.ageTracker.BirthAbsTicks = (long)GenTicks.TicksAbs - pawn.ageTracker.AgeBiologicalTicks;

            //Needs
            pawn.needs.SetInitialLevels();
            //pawn.needs.mood = new Need_Mood(pawn);

            NameTriple name = new NameTriple("Volkov", "Volkov", "Commando");
            pawn.Name = name;
            pawn.story.birthLastName = name.Last;

            pawn.story.childhood = BackstoryFrom(BackstorySlot.Adulthood);
            pawn.story.adulthood = BackstoryFrom(BackstorySlot.Childhood);

            pawn.story.melanin = 0.4f;
            pawn.story.bodyType = BodyTypeDefOf.Male;
            pawn.story.crownType = CrownType.Narrow;
            pawn.story.hairColor = default;
            pawn.story.hairDef = DefDatabase<HairDef>.GetNamed("Shaved");

            pawn.story.traits.GainTrait(new Trait(TraitDefOf.Tough, TraitDefOf.Tough.degreeDatas.First().degree, true));
            pawn.story.traits.GainTrait(new Trait(TraitDefOf.ShootingAccuracy, TraitDefOf.ShootingAccuracy.degreeDatas.First().degree, true));
            pawn.story.traits.GainTrait(new Trait(TraitDefOf.Transhumanist, TraitDefOf.Transhumanist.degreeDatas.First().degree, true));

            //Skills
            var allSkills = DefDatabase<SkillDef>.AllDefsListForReading;
            foreach (var skill in allSkills)
            {
                //TODO: Test Range Only, adjust for finalized value LOOK AT: PawnGenerator.FinalLevelOfSkill
                int skillLevel = Rand.Range(0, 20);
                SkillRecord skillRec = pawn.skills.GetSkill(skill);
                skillRec.Level = skillLevel;
                if(skillRec.TotallyDisabled) continue;
                
            }

            if (pawn.workSettings != null && pawn.Faction != null && pawn.Faction.IsPlayer)
            {
                pawn.workSettings.EnableAndInitialize();
            }

            if (Find.Scenario != null)
            {
                Find.Scenario.Notify_NewPawnGenerating(pawn, PawnGenerationContext.NonPlayer);
            }

            //Gear
            Apparel pants = (Apparel)ThingMaker.MakeThing(ThingDef.Named("Apparel_Pants"), ThingDef.Named("Leather_Panthera"));
            Apparel shirt = (Apparel)ThingMaker.MakeThing(ThingDef.Named("Apparel_CollarShirt"), ThingDef.Named("Leather_Wolf"));
            Apparel jacket = (Apparel)ThingMaker.MakeThing(ThingDef.Named("Apparel_Jacket"), ThingDef.Named("Leather_Bear"));
            pawn.apparel.Wear(pants);
            pawn.apparel.Wear(shirt);
            pawn.apparel.Wear(jacket);
            ThingWithComps teslaGun = (ThingWithComps) ThingMaker.MakeThing(ThingDef.Named("Sovjet_TeslaGun"));
            pawn.equipment.AddEquipment(teslaGun);

            var parts = pawn.def.race.body.AllParts;
            pawn.health.AddHediff(DefDatabase<HediffDef>.GetNamed("AugmentedEye"), parts.FirstOrDefault(p => p.IsInGroup(BodyPartGroupDefOf.Eyes)));
            pawn.health.AddHediff(DefDatabase<HediffDef>.GetNamed("CannonImplant"), parts.FirstOrDefault(p => p.def == BodyPartDefOf.Arm));

            return pawn;
        }

        private static Backstory BackstoryFrom(BackstorySlot slot)
        {
            return new Backstory()
            {
                baseDesc = "Test description",
                slot = slot,
            };
        }

        public static Pawn GenerateChitzkoi(Map map)
        {
            PawnGenerationRequest request = new PawnGenerationRequest()
            {
                Context = PawnGenerationContext.NonPlayer,
                KindDef = RedAlertDefOf.Chitzkoi,
            };
            return PawnGenerator.GeneratePawn(request);
        }
        */
        public static Thing GenTemp()
        {
            throw new System.NotImplementedException();
        }
    }
}
