using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace TiberiumRim
{
    public class CellArea : IExposable
    {
        public void ExposeData()
        {
            
        }

        public IntVec3 this[int i]
        {
            get
            {
                return  IntVec3.Invalid;
            }
            set
            {

            }
        }
    }
}
