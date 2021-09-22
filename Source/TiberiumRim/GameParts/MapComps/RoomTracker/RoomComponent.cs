using Verse;

namespace TiberiumRim
{
    public abstract class RoomComponent
    {
        private RoomTracker parent;

        public RoomTracker Parent => parent;
        public Map Map => Parent.Map;
        public Room Room => Parent.Room;

        public virtual void Create(RoomTracker parent)
        {
            this.parent = parent;
        }

        public virtual void Disband(RoomTracker parent, Map map) { }
        public virtual void Notify_Reused() { }
        public virtual void Notify_RoofClosed() { }
        public virtual void Notify_RoofOpened() { }
        public virtual void Notify_RoofChanged() { }
        public virtual void Notify_ThingSpawned(Thing thing) {}
        public virtual void Notify_ThingDespawned(Thing thing) { }

        public virtual void PreApply() { }

        public virtual void FinalizeApply()
        {
            foreach (var thing in Room.ContainedAndAdjacentThings)
            {
                Notify_ThingSpawned(thing);
            }
        }

        public virtual void CompTick() { }

        public virtual void OnGUI() { }

        public virtual void Draw() { }
    }
}
