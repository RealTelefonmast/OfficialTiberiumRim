using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class FXThing : ThingWithComps, IFXObject
    {
        public ExtendedGraphicData ExtraData => (base.def as FXThingDef).extraData;
        public CompFX FXComp => this.GetComp<CompFX>();

        public virtual Vector3[] DrawPositions => new Vector3[1] { base.DrawPos };
        public virtual Color[] ColorOverrides => new Color[1] { Color.white };
        public virtual float[] OpacityFloats => new float[1] { 1f };
        public virtual float?[] RotationOverrides => new float?[1] { null };
        public virtual bool[] DrawBools => new bool[1] { true };
        public virtual bool ShouldDoEffecters => true;

        public void CondLog(string s)
        {
            if (Find.Selector.IsSelected(this))
            {
                Log.Message(s);
            }
        }
    }
}

