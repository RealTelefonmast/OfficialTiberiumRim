using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeleCore;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Building_TiberiumSpike : FXBuilding
    {
        public Building_TiberiumGeyser boundGeyser;

        public Comp_NetworkStructure CompTNW => this.TryGetComp<Comp_NetworkStructure>();
        public NetworkSubPart TibComponent => CompTNW[TiberiumDefOf.TiberiumNetwork];

        public override bool FX_ShouldDrawAt(int index)
        {
            return index switch
            {
                0 => TibComponent.HasConnection,
                1 => TibComponent.HasConnection && CompTNW.CompPower.PowerOn,
                _ => base.FX_ShouldDrawAt(index)
            };
        }

        public override Color? FX_GetColorAt(int index)
        {
            return index switch
            {
                _ => Color.white
            };
        }

        public override float FX_GetOpacityAt(int index)
        {
            return index switch
            {
                _ => 1f
            };
        }

        //
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            boundGeyser = this.Position.GetFirstThing(map, TiberiumDefOf.TiberiumGeyser) as Building_TiberiumGeyser;
            boundGeyser.Notify_SpikeSpawned(this);
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);
            boundGeyser?.Notify_SpikeDespawned();
        }

        public override IEnumerable<InspectTabBase> GetInspectTabs()
        {
            return base.GetInspectTabs();
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(base.GetInspectString());
            sb.AppendLine("\n" + "TR_GeyserContent" + ": " + boundGeyser.ContentPercent.ToStringPercent());
            return sb.ToString().TrimEndNewlines();
        }
    }
}
