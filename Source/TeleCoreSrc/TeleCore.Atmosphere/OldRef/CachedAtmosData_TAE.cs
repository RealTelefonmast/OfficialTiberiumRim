// Preserved from TeleCore/Caching/CachedAtmosData.cs (DefValueStack<AtmosphericDef> — no TAC counterpart)
using TeleCore;

namespace TeleCore.Atmosphere.OldRef
{
    public struct CachedAtmosData_TAE
    {
        public int roomID;
        public DefValueStack<TAE.AtmosphericDef> stack;

        public CachedAtmosData_TAE()
        {
            roomID = -1;
            stack = new DefValueStack<TAE.AtmosphericDef>();
        }

        public CachedAtmosData_TAE(TAE.RoomComponent_Atmospheric roomComp)
        {
            roomID = roomComp.Room.ID;
            stack = roomComp.Container.ValueStack;
            if (roomComp.IsOutdoors)
            {
                stack += roomComp.OutsideContainer.ValueStack;
            }
        }

        public override string ToString()
        {
            return $"[{roomID}][{stack.Empty}]\n{stack}";
        }
    }
}
