using Verse;

namespace TiberiumRim
{
    public class Comp_PollutionSource : ThingComp, IPollutionSource
    {
        public CompProperties_PollutionSource Props => (CompProperties_PollutionSource) base.props;
        public Thing Thing => parent;
        public Room Room => this.parent.GetRoomIndirect();
        public int PollutionInterval => Props.pollutionInterval;
        public int PollutionAmount => Props.pollutionAmount;
        public bool IsPolluting => (!(parent is Building_TiberiumGeyser g) || Building_TiberiumGeyser.makePollutionGas);

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            Thing.Map.Tiberium().PollutionInfo.RegisterSource(this);
        }

        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            Thing.Map.Tiberium().PollutionInfo.DeregisterSource(this);
        }
    }

    public class CompProperties_PollutionSource : CompProperties
    {
        public int pollutionAmount;
        public int pollutionInterval;

        public CompProperties_PollutionSource()
        {
            this.compClass = typeof(Comp_PollutionSource);
        }
    }
}
