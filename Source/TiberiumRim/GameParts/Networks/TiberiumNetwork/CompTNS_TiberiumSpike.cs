using TeleCore;
using TeleCore.Data.Events;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class CompTNS_TiberiumSpike : Comp_TiberiumNetworkStructure
    {
        //FX
        public override bool? FX_ShouldDraw(FXLayerArgs args)
        {
            return args.index switch
            {
                0 => HasConnection,
                1 => HasConnection && CompPower.PowerOn,
                _ => base.FX_ShouldDraw(args)
            };
        }

        public override Color? FX_GetColor(FXLayerArgs args) => Color.white;

        public override float? FX_GetOpacity(FXLayerArgs args) => 1f;

        
        //
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
        }

        public override string CompInspectStringExtra()
        {
            //StringBuilder sb = new StringBuilder();
            //sb.AppendFormat(base.CompInspectStringExtra());
            //sb.AppendLine("\n" + "TR_GeyserContent" + ": " + boundGeyser.ContentPercent.ToStringPercent());
            return base.CompInspectStringExtra();  //sb.ToString().TrimEndNewlines();
        }
    }
}
