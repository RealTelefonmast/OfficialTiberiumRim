using System.Collections.Generic;
using Verse;

namespace TR
{
    // A list of linked mechs for a mech station
    public class MechLink : IExposable
    {
        private List<MechanicalPawn> connectedMechs = new List<MechanicalPawn>();
        private readonly int capacity;

        public List<MechanicalPawn> LinkedMechs => connectedMechs;
        public bool CanHaveNewMech => capacity <= 0 || connectedMechs.Count < capacity;

        public int Count => LinkedMechs.Count;

        public MechLink(int capacity)
        {
            if (capacity > 0)
                this.capacity = capacity;
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref connectedMechs, "connectedMechs", LookMode.Reference);
        }

        public bool Contains(MechanicalPawn mech)
        {
            return connectedMechs.Contains(mech);
        }

        public bool TryTransferTo(MechLink other, MechanicalPawn mech)
        {
            if (other.TryConnectNewMech(mech))
            {
                this.RemoveMech(mech);
                return true;
            }
            return false;
        }

        public bool TryConnectNewMech(MechanicalPawn mech)
        {
            if (!CanHaveNewMech) return false;
            connectedMechs.Add(mech);
            return true;
        }

        public void RemoveMech(MechanicalPawn mech)
        {
            connectedMechs.Remove(mech);
        }

        public MechanicalPawn this[int i] => LinkedMechs[i];
    }
}
