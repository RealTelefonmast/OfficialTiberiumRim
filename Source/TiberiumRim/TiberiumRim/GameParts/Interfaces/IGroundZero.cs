using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld.Planet;
using Verse;

namespace TiberiumRim
{
    public interface IGroundZero
    {
        public bool IsGroundZero { get; }

        public Thing GZThing { get; }

    }
}
