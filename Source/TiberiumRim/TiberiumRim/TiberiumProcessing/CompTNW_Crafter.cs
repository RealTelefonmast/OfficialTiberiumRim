using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class CompTNW_Crafter : CompTNW
    {
        public new Building_WorkTable parent;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            parent = base.parent as Building_WorkTable;           
        }

        public bool IsWorkedOn => CurBill != null;

        public TiberiumBill CurBill
        {
            get
            {
                return (TiberiumBill)parent.billStack.Bills.Find(b => b is TiberiumBill tb && tb.isBeingDone);
            }
        }

        public Color CurColor => CurBill?.BillColor ?? Color.clear;

        public override Color[] ColorOverrides => new Color[] { CurColor, Color.white, Color.white };
        public override float[] OpacityFloats => new float[] { 1f, 1f, 1f };
        public override bool[] DrawBools => new bool[] { IsWorkedOn, base.DrawBools[1], true };
        public override bool ShouldDoEffecters => IsWorkedOn;
    }
}
