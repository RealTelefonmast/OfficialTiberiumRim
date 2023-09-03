using System.Collections.Generic;
using TeleCore;
using TeleCore.Data.Events;
using Verse;

namespace TR
{
    public class Building_GrowthSuppressor : FXBuilding
    {
        public int tick = 0; 

        public bool[] bools = new bool[3];

        //FX
        public override float? FX_GetOpacity(FXLayerArgs args)
        {
            return args.index switch
            {
                0 => 1f,
                1 => 1f,
                2 => 1f,
                3 => 1f,
                _ => base.FX_GetOpacity(args)
            };
        }

        public override bool? FX_ShouldDraw(FXLayerArgs args)
        {
            return args.index switch
            {
                0 => true,
                1 => bools[0],
                2 => bools[1],
                3 => true,
                _ => base.FX_ShouldDraw(args)
            };
        }

        public override void Tick()
        {
            base.Tick();
            
            //
            tick++;
            if (tick < 100)
            {
                bools[0] = true;
                return;
            } 
            if (tick < 200)
            {
                bools[1] = true;
                return;
            }
            if (tick < 300)
            {
                bools[0] = false;
                bools[1] = false;
                return;
            }
            tick = 0;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
                yield return g;
        }
    }
}
