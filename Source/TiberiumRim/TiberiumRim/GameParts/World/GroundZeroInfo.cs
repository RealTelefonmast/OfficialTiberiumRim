using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld.Planet;
using Verse;

namespace TiberiumRim
{
    public class GroundZeroInfo : WorldInfo
    {
        private Thing groundZeroThing;

        private IGroundZero MainGroundZero;

        public GroundZeroInfo(World world) : base(world) { }

        public override void ExposeData()
        {
            Scribe_References.Look(ref groundZeroThing, "gzThing");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                MainGroundZero = GetGroundZeroAfterLoad();
            }
        }

        private IGroundZero GetGroundZeroAfterLoad()
        {
            return (IGroundZero)groundZeroThing;
        }

        public bool IsGroundZero(IGroundZero groundZero)
        {
            return MainGroundZero == groundZero;
        }

        public bool HasGroundZero => MainGroundZero != null;

        public void TryRegisterGroundZero(IGroundZero groundZero)
        {
            if (MainGroundZero != null) return;
            MainGroundZero = groundZero;
            groundZeroThing = MainGroundZero.GZThing;
        }
    }
}
