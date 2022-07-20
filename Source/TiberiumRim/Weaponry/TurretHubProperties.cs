using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    public class TurretHubProperties
    {
        public bool isHub = false;
        public ThingDef hubDef;
        public ThingDef turretDef;
        public GraphicData cableGraphic;
        public string cableTexturePath;
        public int maxTurrets = 3;
        public float connectRadius = 7.9f;
    }
}
