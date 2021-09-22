using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    public class ContainerProperties
    {
        public int maxStorage = 0;
        public bool dropContents = false;
        public bool leaveContainer = false;
        public bool doExplosion = false;
        public float explosionRadius = 6;
    }
}
