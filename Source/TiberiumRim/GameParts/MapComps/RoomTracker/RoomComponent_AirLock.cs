using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class RoomComponent_AirLock : RoomComponent
    {
        private RoomComponent_Pollution pollutionCompInt;
        private List<Comp_ANS_AirVent> AirVents = new List<Comp_ANS_AirVent>();
        private List<Building_AirLock> AirLockDoors = new List<Building_AirLock>();
        
        public RoomComponent_Pollution Pollution => pollutionCompInt ??= Parent.GetRoomComp<RoomComponent_Pollution>();
        public bool IsActive => this.Room.Districts.All(r => r.Room.Role == TiberiumDefOf.TR_AirLock);
        public bool IsClean => Pollution.ActualPollution <= 0;

        public bool AllDoorsClosed => AirLockDoors.All(d => !d.Open);

        public override void Create(RoomTracker parent)
        {
            base.Create(parent);
            Log.Message("Creating AirLock RoomComp");

            foreach (var thing in Room.ContainedAndAdjacentThings)
            {
                TryAddComponent(thing);
            }
        }

        public override void Notify_ThingSpawned(Thing thing)
        {
            Log.Message($"Spawned {thing} in {this.Room.ID} RoomComp_AirLock");
            TryAddComponent(thing);
        }

        public override void Notify_ThingDespawned(Thing thing)
        {
            base.Notify_ThingDespawned(thing);
            AirVents.RemoveAll(t => t.Thing == thing);
        }

        private void TryAddComponent(Thing thing)
        {
            if (thing is Building_AirLock airLock)
            {
                AirLockDoors.Add(airLock);
                return;
            }
            var comp = thing.TryGetComp<Comp_ANS_AirVent>();
            if (comp != null)
            {
                AirVents.Add(comp);
            }
        }


        public override void Draw()
        {
            if (Room.Cells.Contains(UI.MouseCell()))
            {
                GenDraw.DrawFieldEdges(AirVents.Select(t => t.Thing.Position).ToList(), Color.green);
            }

        }
    }
}