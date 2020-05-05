using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Building_ANGLETEST : Building
    {
        private IntVec3 AVector;
        private IntVec3 BVector;

        public override void Draw()
        {
            base.Draw();
            GenDraw.DrawLineBetween(DrawPos, AVector.ToVector3Shifted(), SimpleColor.Red);
            GenDraw.DrawLineBetween(DrawPos, BVector.ToVector3Shifted(), SimpleColor.Blue);
            GenDraw.DrawLineBetween(AVector.ToVector3Shifted(), BVector.ToVector3Shifted(), SimpleColor.Green);
        }

        //North = 0° when (A - DrawPos).AngleFlat()
        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            Vector3 A = AVector.ToVector3Shifted();
            Vector3 B = BVector.ToVector3Shifted();
            sb.AppendLine("Center: " + Position + " | A: " + AVector + " | B: " + BVector);
            sb.AppendLine("C-A: " + (DrawPos - A) + " | " + (DrawPos - A).AngleFlat() + " | " + (DrawPos - A).ToAngleFlat());
            sb.AppendLine("A-C: " + (A - DrawPos) + " | " + (A - DrawPos).AngleFlat() + " | " + (A - DrawPos).ToAngleFlat());
            sb.AppendLine("AngleFlats: " + DrawPos.AngleFlat() + "," + DrawPos.ToAngleFlat() +
                          " | " + A.AngleFlat() + "," + A.ToAngleFlat() +
                          " | " + B.AngleFlat() + "," + B.ToAngleFlat());
            sb.AppendLine("Angle Center-A: " + DrawPos.AngleToFlat(A));
            sb.AppendLine("Angle Center-B: " + DrawPos.AngleToFlat(B));
            sb.AppendLine("Angle A-B: " + A.AngleToFlat(B));
            sb.AppendLine("Angle B-A: " + B.AngleToFlat(A));
            return sb.ToString();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            yield return new Command_Action
            {
                defaultLabel = "Set A",
                action = delegate
                {
                    Find.Targeter.BeginTargeting(new TargetingParameters
                        {
                            canTargetBuildings = true,
                            canTargetFires = false,
                            canTargetItems = false,
                            canTargetLocations = true,
                            canTargetPawns = false,
                            canTargetSelf = false,
                        },
                        delegate (LocalTargetInfo target)
                        {
                            AVector = target.Cell;

                        }, null, null, null);
                }
            };

            yield return new Command_Action
            {
                defaultLabel = "Set B",
                action = delegate
                {
                    Find.Targeter.BeginTargeting(new TargetingParameters
                        {
                            canTargetBuildings = true,
                            canTargetFires = false,
                            canTargetItems = false,
                            canTargetLocations = true,
                            canTargetPawns = false,
                            canTargetSelf = false,
                        },
                        delegate (LocalTargetInfo target)
                        {
                            BVector = target.Cell;

                        }, null, null, null);
                }
            };
        }
    }
}
