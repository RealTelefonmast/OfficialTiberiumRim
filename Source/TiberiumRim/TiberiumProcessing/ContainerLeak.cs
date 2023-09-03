using System.Collections.Generic;
using TeleCore;
using Verse;

namespace TR
{
    public class ContainerLeak
    {
        private Comp_Network parent;
        private HashSet<IntVec3> radiationCells;

        public float Severity
        {
            get
            {
                
                return 0f;
            }
        }

        public void Tick()
        {

        }

        public void SetRadiationRadius()
        {

        }
    }
}
