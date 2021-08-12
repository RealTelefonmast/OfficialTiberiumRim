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
    public class Comp_NetworkStructureCrafter : Comp_NetworkStructure
    {
        public new Building_WorkTable parent;
        public TiberiumBillStack billStack;

        //CompFX
        public Color CurColor => Color.clear;//CurBill?.BillColor ?? Color.clear;
        public override Color[] ColorOverrides => new Color[] { CurColor, Color.white, Color.white };
        public override float[] OpacityFloats => new float[] { 1f, 1f, 1f };
        public override bool[] DrawBools => new bool[] { IsWorkedOn, base.DrawBools[1], true };
        public override bool ShouldDoEffecters => IsWorkedOn;

        //Crafter Code
        public bool IsWorkedOn => BillStack.CurrentBill != null;
        public TiberiumBillStack BillStack => billStack;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            parent = base.parent as Building_WorkTable;
            if (!respawningAfterLoad)
                billStack = new TiberiumBillStack(this);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look(ref billStack, "tiberiumBillStack", this);
        }
    }
}
