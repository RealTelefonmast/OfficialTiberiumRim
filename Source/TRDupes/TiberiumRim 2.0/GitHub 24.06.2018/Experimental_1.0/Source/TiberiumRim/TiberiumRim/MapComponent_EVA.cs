using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace TiberiumRim
{
    public class MapComponent_EVA : MapComponent
    {
        public MapComponent_EVA(Map map) : base(map)
        {
        }

        

        public EVA EVA
        {
            get
            {
                //Currently only GDI
                return EVA.GDI;
            }
        }
    }

    public enum EVA
    {
        GDI,
        Nod,
        Scrin
    }
}
