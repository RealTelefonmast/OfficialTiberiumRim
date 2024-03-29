﻿using System;
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
        public TiberiumBillStack billStack;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            parent = base.parent as Building_WorkTable;
            if(!respawningAfterLoad)
                billStack = new TiberiumBillStack(this);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look(ref billStack, "tiberiumBillStack", this);
        }

        public bool IsWorkedOn => false; //CurBill != null;

        public TiberiumBillStack BillStack => billStack;

        //public CustomTiberiumBill CurrentTiberiumBill => BillStack.Bills.Find(b => b);

        public Color CurColor => Color.clear;//CurBill?.BillColor ?? Color.clear;

        public override Color[] ColorOverrides => new Color[] { CurColor, Color.white, Color.white };
        public override float[] OpacityFloats => new float[] { 1f, 1f, 1f };
        public override bool[] DrawBools => new bool[] { IsWorkedOn, base.DrawBools[1], true };
        public override bool ShouldDoEffecters => IsWorkedOn;
    }
}
