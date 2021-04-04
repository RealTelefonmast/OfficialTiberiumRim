using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
