using System.Collections.Generic;
using Verse;

namespace TiberiumRim
{
    public abstract class RoomComponent
    {
        private RoomTracker parent;
        protected HashSet<Pawn> containedPawns;

        public RoomTracker Parent => parent;
        public Map Map => Parent.Map;
        public Room Room => Parent.Room;

        public bool Disbanded => Parent.IsDisbanded;

        public virtual void Create(RoomTracker parent)
        {
            this.parent = parent;
            containedPawns = new HashSet<Pawn>();
        }

        public virtual void Disband(RoomTracker parent, Map map) { }
        public virtual void Notify_Reused() { }
        public virtual void Notify_RoofClosed() { }
        public virtual void Notify_RoofOpened() { }
        public virtual void Notify_RoofChanged() { }
        public virtual void Notify_ThingSpawned(Thing thing) {}
        public virtual void Notify_ThingDespawned(Thing thing) { }

        public virtual void Notify_PawnEnteredRoom(Pawn pawn)
        {
            containedPawns.Add(pawn);
        }

        public virtual void Notify_PawnLeftRoom(Pawn pawn)
        {
            containedPawns.Remove(pawn);
        }

        public bool ContainsPawn(Pawn pawn)
        {
            return containedPawns.Contains(pawn);

        }

        public virtual void PreApply() { }

        public virtual void FinalizeApply()
        {
        }

        public virtual void CompTick() { }

        public virtual void OnGUI() { }

        public virtual void Draw() { }

        public override string ToString()
        {
            return $"{nameof(this.GetType)}[{Room.ID}]";
        }
    }
}
