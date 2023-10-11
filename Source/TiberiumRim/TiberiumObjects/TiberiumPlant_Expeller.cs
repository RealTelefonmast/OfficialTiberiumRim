using TAC;
using TeleCore;

namespace TR
{
    public class TiberiumPlant_Expeller : TiberiumPlant
    {
        //Plant which expells a cloud of tiberium gas at nearby pawns

        public override void TickLong()
        {
            base.TickLong();
            MapHeld.GetMapInfo<SpreadingGasGrid>().Notify_SpawnGasAt(Position, TiberiumDefOf.TiberiumPollution, 255);
        }
    }
}
