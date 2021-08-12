using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Building_NetworkStructureTransmitter : FXBuilding
    {
        public override Graphic Graphic => TiberiumContent.TiberiumNetworkPipes;
        public Comp_NetworkStructureTransmitter TransmitterComp => this.TryGetComp<Comp_NetworkStructureTransmitter>();

        public override void Draw()
        {
            base.Draw();
            if (TransmitterComp.Network != null)
            {
                //TODO: colorize...
                //Color color = TransmitterComp.Network;
                //TiberiumContent.TiberiumNetworkPipesGlow.ColoredVersion(ShaderDatabase.MoteGlow, color, color).Draw(this.DrawPos + Altitudes.AltIncVect, Rotation, this);
            }
        }
    }
}
