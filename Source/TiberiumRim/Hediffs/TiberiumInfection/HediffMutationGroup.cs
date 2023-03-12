using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using RimWorld;
using Verse;

namespace TiberiumRim
{

    public class HediffFloat
    {
        public HediffDef hediffDef;
        public float value;

        public virtual void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            string s = Regex.Replace(xmlRoot.FirstChild.Value, @"\s+", "");
            string[] array = s.Split(',');
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "hediffDef", array[0], null, null);
            if (array.Length > 1)
                this.value = (float)ParseHelper.FromString(array[1], typeof(float));
        }
    }

    public class HediffMutationGroup : Def
    {
        public HediffDef Generic;
        public HediffDef Skin;
        public HediffDef Arm;
        public HediffDef Leg;
        public HediffDef Hand;
        public HediffDef Foot;
        public HediffDef Torso;
        public HediffDef Spine;
        public List<HediffFloat> Organ;
        public HediffDef Bone;

        public float MutationPct(Pawn pawn)
        {
            int count = pawn.health.hediffSet.hediffs.Count(IsOfMutation);
            return count / (float)pawn.HealthComp().NonMisingPartsCount;
        }

        public bool IsOfMutation(Hediff hediff)
        {
            return hediff.def == Generic ||
                hediff.def == Skin       ||
                hediff.def == Arm        ||
                hediff.def == Leg        ||
                hediff.def == Hand       ||
                hediff.def == Foot       ||
                hediff.def == Torso      ||
                hediff.def == Spine      ||
                hediff.def == Bone       ||
                (Organ?.Any(d => hediff.def == d.hediffDef) ?? false);
        }

        public HediffDef HediffFor(BodyPartRecord part)
        {
            if (part == null)
                return null;
            var tags = part.def.tags;
            HediffDef hediff = null;

            hediff = Generic;
            if (part.depth == BodyPartDepth.Outside)
            {
                //Skin
                if(!part.IsOrgan())
                    hediff = Skin;

                //Arm
                if (tags.Contains(BodyPartTagDefOf.ManipulationLimbCore))
                {
                    hediff = Arm;
                }
                //Hand
                if (tags.Contains(BodyPartTagDefOf.ManipulationLimbSegment))
                {
                    hediff = Hand;
                }
                //Leg
                if (part.IsInGroup(BodyPartGroupDefOf.Legs))
                {
                    if (tags.Contains(BodyPartTagDefOf.MovingLimbCore))
                    {
                        hediff = Leg;
                    }
                    //Foot
                    if (tags.Contains(BodyPartTagDefOf.MovingLimbSegment))
                    {
                        hediff = Foot;
                    }
                }

                //
                if (part.IsInGroup(BodyPartGroupDefOf.Torso))
                {
                    hediff = Torso;
                }
            }

            if (part.depth == BodyPartDepth.Inside)
            {
                //Bone
                if (part.def.IsSolidInDefinition_Debug && part.def.bleedRate == 0 && part.def.IsSkinCoveredInDefinition_Debug)
                {
                    hediff = Bone;
                }
                //Spine
                if (tags.Contains(BodyPartTagDefOf.Spine))
                {
                    hediff = Spine;
                }
                //Organs
                if (part.IsOrgan())
                {
                    hediff = Organ.RandomElementByWeight(t => t.value).hediffDef;
                }
            }
            return hediff;
        }
    }
}
