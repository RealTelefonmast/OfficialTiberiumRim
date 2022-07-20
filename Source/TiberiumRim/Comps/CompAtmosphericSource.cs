using TeleCore;
using Verse;

namespace TiberiumRim
{
    public class CompAtmosphericSource : ThingComp, IAtmosphericSource
    {
        public CompProperties_PollutionSource Props => (CompProperties_PollutionSource) base.props;
        public Thing Thing => parent;
        public Room Room => this.parent.GetRoomIndirect();
        public NetworkValueDef AtmosphericType => TiberiumDefOf.TibPollution;
        
        public bool IsActive => (!(parent is Building_TiberiumGeyser g) || Building_TiberiumGeyser.makePollutionGas);
        public int CreationInterval => Props.pollutionInterval;
        public int CreationAmount => Props.pollutionAmount;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            Thing.Map.Tiberium().AtmosphericInfo.RegisterSource(this);
        }

        public override void PostDeSpawn(Map map)
        {
            map.Tiberium().AtmosphericInfo.DeregisterSource(this);
            base.PostDeSpawn(map);
        }
    }

    public class CompProperties_PollutionSource : CompProperties
    {
        public int pollutionAmount;
        public int pollutionInterval;

        public CompProperties_PollutionSource()
        {
            this.compClass = typeof(CompAtmosphericSource);
        }
    }
}
