using Verse;

namespace TiberiumRim
{
    public interface IMechGarage<T> where T : MechanicalPawn
    {
        MechGarage MainGarage { get; }

        void SendToGarage(T mech);
        T ReleaseFromGarage(T mech, Map map, IntVec3 pos, ThingPlaceMode placeMode = ThingPlaceMode.Direct);
    }
}
