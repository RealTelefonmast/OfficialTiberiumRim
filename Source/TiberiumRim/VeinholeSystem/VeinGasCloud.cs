using Verse;

namespace TiberiumRim
{
    public class VeinGasCloud : HomingThing
    {
        public override void Tick()
        {
            base.Tick();
            foreach (var intVec3 in Position.CellsAdjacent8Way())
            {
                var pawn = intVec3.GetFirstPawn(Map);
                if(pawn != null) 
                    HediffUtils.TryInfectPawn(pawn, 1, true, 1);
            }
        }
    }
}
