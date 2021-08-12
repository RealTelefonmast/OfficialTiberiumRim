using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
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

            GenDraw.DrawFieldEdges(new List<IntVec3>(){AVector}, Color.blue);
            GenDraw.DrawFieldEdges(new List<IntVec3>() { BVector }, Color.magenta);
            GenDraw.DrawFieldEdges(falseBools, Color.red);
            GenDraw.DrawFieldEdges(trueBools, Color.green);
            GenDraw.DrawFieldEdges(falseBools, Color.red);

            foreach (var vec in vecs)
            {
                //GenDraw.DrawCircleOutline(vec + new Vector3(0, AltitudeLayer.MetaOverlays.AltitudeFor(), 0), 0.2f, SimpleColor.Red);
            }
            //GenDraw.DrawLineBetween(DrawPos, AVector.ToVector3Shifted(), SimpleColor.Red);
            //GenDraw.DrawLineBetween(DrawPos, BVector.ToVector3Shifted(), SimpleColor.Blue);
            //GenDraw.DrawLineBetween(AVector.ToVector3Shifted(), BVector.ToVector3Shifted(), SimpleColor.Green);
        }

        public bool isInCorner;
        public bool isOutCorner;

        public IntVec3 origin;
        public IntVec3 c;
        public IntVec3 diff;

        public IntVec3[] inCorner;
        public IntVec3[] outCorner;

        public List<IntVec3> trueBools = new List<IntVec3>();
        public List<IntVec3> falseBools = new List<IntVec3>();

        private void CheckCorner(IntVec3 origin, IntVec3 c)
        {
            trueBools.Clear();
            falseBools.Clear();

            this.origin = origin;
            this.c = c;

            diff = c - origin;
            if (diff.x == 0 || diff.z == 0)
            {

                return;
            }
            inCorner = new IntVec3[] { new IntVec3(diff.x, 0, 0), new IntVec3(0, 0, diff.z) };
            outCorner = new IntVec3[] { new IntVec3(diff.x, 0, 0), new IntVec3(0, 0, diff.z) };


            Predicate<IntVec3> FitsIn = c =>
            {
                var edifice = c.GetEdifice(Map);
                return edifice != null;
            };

            Predicate<IntVec3> FitsOut = c =>
            {
                var edifice = c.GetEdifice(Map);
                return edifice == null;
            };

            isOutCorner = FitsOut(c + outCorner[0]) && FitsOut(c + outCorner[1]);
            isInCorner = FitsIn(c + inCorner[0]) && FitsIn(c + inCorner[1]);
        }

        private HashSet<Vector3> cornerData = new HashSet<Vector3>();

        private Vector2[] vecs;

        public void GetBorderData(IntVec3 origin, IntVec3 borderCell)
        {
            Vector2[] vecsOrigin = origin.CornerVecs();
            Vector2[] vecsBorder = borderCell.CornerVecs();

            this.vecs = vecsOrigin.Intersect(vecsBorder).ToArray();
        }


        //North = 0° when (A - DrawPos).AngleFlat()
        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Origin: " + origin);
            sb.AppendLine("C: " + c);
            sb.AppendLine("Diff: " + diff);
            sb.AppendLine("IsOutCorner: " + isOutCorner);
            sb.AppendLine("IsInCorner: " + isInCorner);

            sb.AppendLine("inData: " + inCorner[0] + "|" + inCorner[1]);
            sb.AppendLine("outData: " + outCorner[0] + "|" + outCorner[1]);

            sb.AppendLine("BORDER VECTORS: " + vecs[0] + "|" + vecs[1]);

            /*
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
            */
            return sb.ToString();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            yield return new Command_Action
            {
                defaultLabel = "Set Origin",
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
                defaultLabel = "Set C",
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

            yield return new Command_Action
            {
                defaultLabel = "Update",
                action = delegate
                {
                    CheckCorner(AVector, BVector);
                    GetBorderData(AVector, BVector);
                }
            };
        }
    }
}
