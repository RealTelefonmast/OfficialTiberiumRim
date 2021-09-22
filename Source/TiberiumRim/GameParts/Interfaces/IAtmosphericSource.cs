using Verse;

namespace TiberiumRim
{
    public interface IAtmosphericSource
    {
        public Thing Thing { get; }
        public Room Room { get; }
        public NetworkValueDef AtmosphericType { get; }
        bool IsActive { get; }
        int CreationInterval { get; }
        int CreationAmount { get; }
    }
}
