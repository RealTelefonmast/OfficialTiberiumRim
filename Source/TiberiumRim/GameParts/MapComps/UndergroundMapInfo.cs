using Verse;

namespace TiberiumRim
{
    public class UndergroundMapInfo : MapInformation
    {
        public UndergroundMapInfo(Map map) : base(map)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }
    }

    public class UndergroundResourceLayer : IExposable
    {
        private int layerDepth;

        public void ExposeData()
        {

        }
    }

    public class UndergroundResourceDef : Def
    {

    }
}
