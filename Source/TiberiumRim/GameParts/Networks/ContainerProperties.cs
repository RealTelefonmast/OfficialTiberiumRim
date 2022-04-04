using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    public class ContainerProperties : IExposable
    {
        public int maxStorage = 0;
        public bool dropContents = false;
        public bool leaveContainer = false;
        public bool doExplosion = false;
        public float explosionRadius = 6;

        public void ExposeData()
        {
            Scribe_Values.Look(ref maxStorage, "maxStorage");
            Scribe_Values.Look(ref dropContents, "dropContents");
            Scribe_Values.Look(ref leaveContainer, "leaveContainer");
            Scribe_Values.Look(ref doExplosion, "doExplosion");
            Scribe_Values.Look(ref explosionRadius, "explosionRadius");
        }
    }
}
