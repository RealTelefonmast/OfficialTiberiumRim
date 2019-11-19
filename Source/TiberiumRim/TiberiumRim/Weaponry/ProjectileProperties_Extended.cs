using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class ProjectileProperties_Extended
    {
        
    }

    public class LaserProperties
    {
        public string beamPath;
        public ShaderTypeDef shader = DefDatabase<ShaderTypeDef>.GetNamed("MoteGlow");
        public LaserSourceGlow glow;
        public List<ThingDef> impactMotes;
    }

    public class LaserSourceGlow
    {
        public ThingDef glowMote;
        public float scale = 1;
        public float rotation = 1;
        public float rotationRate = 1;
    }
}
