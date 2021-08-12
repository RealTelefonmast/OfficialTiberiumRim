using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TiberiumRim
{
    public class RoomTracker
    {
        private bool cachedOutside = false;
        private int cachedCellCount = 0;
        private int cachedOpenRoofCount = 0;

        private bool lastRoofBool = false;
        private bool currentRoofBool = false;

        private bool isDisbandedInt = false;

        private Map cachedMap;

        private Room attachedRoom;
        private List<RoomComponent> comps = new List<RoomComponent>();

        private static readonly List<Type> SubClasses = typeof(RoomComponent).AllSubclassesNonAbstract().ToList();

        public bool IsDisbanded => isDisbandedInt;
        public bool IsOutside => cachedOutside;
        public int CellCount => cachedCellCount;
        public int OpenRoofCount => cachedOpenRoofCount;

        public Map Map => cachedMap;
        public Room Room => attachedRoom;

        public RoomTracker(Room room)
        {
            this.attachedRoom = room;

            //Get Group Data
            UpdateGroupData();

            foreach (var type in SubClasses)
            {
                var comp = (RoomComponent)Activator.CreateInstance(type);
                comp.Create(this);
                comps.Add(comp);
            }
        }

        public HashSet<IntVec3> GetBorderCells()
        {
            HashSet<IntVec3> newSet = new HashSet<IntVec3>();
            foreach (IntVec3 c in Room.Cells)
            {
                for (int i = 0; i < 4; i++)
                {
                    IntVec3 cardinal = c + GenAdj.CardinalDirections[i];
                    var region = cardinal.GetRegion(Map);
                    if (region == null || region.Room != Room)
                    {
                        newSet.Add(cardinal);
                    }
                }
            }
            return newSet;
        }

        public T GetRoomComp<T>() where T : RoomComponent
        {
            foreach (var comp in comps)
            {
                if (comp is T t)
                {
                    return t;
                }
            }
            return null;
        }

        public void MarkDisbanded()
        {
            isDisbandedInt = true;
        }

        public void Disband(Map onMap)
        {
            foreach (var comp in comps)
            {
                comp.Disband(this, onMap);
            }
        }

        public void Notify_Reused()
        {
            UpdateGroupData();
            foreach (var comp in comps)
            {
                comp.Notify_Reused();
            }
        }

        public void PreApply()
        {
            foreach (var comp in comps)
            {
                comp.PreApply();
            }
        }

        public void FinalizeApply()
        {
            foreach (var comp in comps)
            {
                comp.FinalizeApply();
            }
        }

        public void Notify_RoofChanged()
        {
            UpdateGroupData();
            //Check if room closed
            if (!lastRoofBool && currentRoofBool)
            {
                RoofClosed();
            }
            if (lastRoofBool && !currentRoofBool)
            {
                RoofOpened();
            }
            foreach (var comp in comps)
            {
                comp.Notify_RoofChanged();
            }
        }

        private void RoofClosed()
        {
            foreach (var comp in comps)
            {
                comp.Notify_RoofClosed();
            }
        }

        private void RoofOpened()
        {
            foreach (var comp in comps)
            {
                comp.Notify_RoofOpened();
            }
        }

        public void RoomTick()
        {
            foreach (var comp in comps)
            {
                comp.CompTick();
            }
        }

        public void RoomOnGUI()
        {
            foreach (var comp in comps)
            {
                comp.OnGUI();
            }
        }

        public void RoomDraw()
        {
            foreach (var comp in comps)
            {
                comp.Draw();
            }
        }

        private void UpdateGroupData()
        {
            cachedMap = Room.Map;
            cachedOutside = Room.UsesOutdoorTemperature;
            cachedCellCount = Room.CellCount;
            if (!cachedOutside)
            {
                //If not outside, we want to know if there are any open roof cells (implies: small room with a few open roof cells
                cachedOpenRoofCount = Room.OpenRoofCount;
            }

            lastRoofBool = currentRoofBool;
            currentRoofBool = !IsOutside;
        }
    }
}
