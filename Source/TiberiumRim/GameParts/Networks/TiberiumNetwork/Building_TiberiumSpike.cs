using System.Collections.Generic;
using System.Text;
using TeleCore;
using TeleCore.Data.Events;
using TeleCore.Network.Data;
using UnityEngine;
using Verse;

namespace TR
{
    public class Building_TiberiumSpike : FXBuilding
    {
        public Building_TiberiumGeyser boundGeyser;

        public Comp_Network CompTNW => this.TryGetComp<Comp_Network>();
        public INetworkPart TibComponent => CompTNW[TiberiumDefOf.TiberiumNetwork];

        public override bool? FX_ShouldDraw(FXLayerArgs args)
        {
            return args.index switch
            {
                0 => TibComponent.HasConnection,
                1 => TibComponent.HasConnection && CompTNW.CompPower.PowerOn,
                _ => base.FX_ShouldDraw(args)
            };
        }

        public override Color? FX_GetColor(FXLayerArgs args)
        {
            return null;
            return args.index switch
            {
                _ => Color.white
            };
        }

        public override float? FX_GetOpacity(FXLayerArgs args)
        {
            return null;
            return base.FX_GetOpacity(args);
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
