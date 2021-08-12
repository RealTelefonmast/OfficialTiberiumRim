using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public interface IParticle
    {
        public void DoInitEvent();
        public void DoFinalEvent();

        public bool ShouldMove { get; }
        public bool ShouldFinalize { get; }
        public Vector3 ExactPos { get; }
        public IntVec3 CellPos { get; }
        public Vector3 DirectionVector { get; set; }
    }
}
