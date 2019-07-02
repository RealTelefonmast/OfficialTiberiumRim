using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class MapComponent_Pollution : MapComponent
    {
        private float pollutionPct = 0f;
        private BoolGrid pollutionGrid;
        

        public MapComponent_Pollution(Map map) : base(map)
        {
        }

        public float CurrentPollution
        {
            get
            {
                return pollutionPct;
            }
            set
            {
                pollutionPct += value;
                pollutionPct = Mathf.Clamp01(pollutionPct);
            }
        }
    }
}
