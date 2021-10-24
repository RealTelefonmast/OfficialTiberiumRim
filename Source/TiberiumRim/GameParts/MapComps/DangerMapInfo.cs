using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    /// <summary>
    /// Implements a custom dynamic danger grid to handle custom danger sources
    /// </summary>
    public class DangerMapInfo : MapInformation
    {
        public DangerMapInfo(Map map) : base(map)
        {
        }

        public void RegisterDanger(CellArea area)
        {

        }

        public void RegisterDanger(IntVec3 center, float area)
        {

        }

        public void RegisterDanger(IntVec3 cell)
        {

        }
    }
}
