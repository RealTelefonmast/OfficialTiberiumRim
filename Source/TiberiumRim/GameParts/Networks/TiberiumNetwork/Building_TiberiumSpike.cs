using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Building_TiberiumSpike : FXBuilding
    {
        public Building_TiberiumGeyser boundGeyser;

        public Comp_NetworkStructure CompTNW => this.TryGetComp<Comp_NetworkStructure>();
        public NetworkComponent TibComponent => CompTNW[TiberiumDefOf.TiberiumNetwork];

        public override float[] OpacityFloats => new float[] { 1f, 1f };
        public override bool[] DrawBools => new bool[] { TibComponent.HasConnection, TibComponent.HasConnection && CompTNW.CompPower.PowerOn };
        public override Color[] ColorOverrides => new Color[] { Color.white, Color.white };

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
