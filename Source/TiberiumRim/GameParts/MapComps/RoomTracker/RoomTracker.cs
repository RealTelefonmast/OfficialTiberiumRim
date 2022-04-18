using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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

        //Shared Room Data
        private IntVec3[] borderCells = new IntVec3[]{};
        private IntVec3[] thinRoofCells = new IntVec3[]{};

        private IntVec3[] cornerCells = new IntVec3[4];
        private IntVec3 minVec;
        private IntVec2 size;

        private Vector3 actualCenter;
        private Vector3 drawPos;

        private static readonly List<Type> SubClasses = typeof(RoomComponent).AllSubclassesNonAbstract().ToList();

        public bool IsDisbanded => isDisbandedInt;
        public bool IsOutside => cachedOutside;
        public int CellCount => cachedCellCount;
        public int OpenRoofCount => cachedOpenRoofCount;

        public Map Map => cachedMap;
        public Room Room => attachedRoom;

        public IntVec3[] BorderCellsNoCorners => borderCells;
        public IntVec3[] ThinRoofCells => thinRoofCells;

        public IntVec3[] MinMaxCorners => cornerCells;
        public IntVec3 MinVec => minVec;
        public IntVec2 Size => size;
        public Vector3 ActualCenter => actualCenter;
        public Vector3 DrawPos => drawPos;

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

        public void Notify_ThingSpawned(Thing thing)
        {
            foreach (var comp in comps)
            {
                comp.Notify_ThingSpawned(thing);
            }
        }

        public void Notify_ThingDespawned(Thing thing)
        {
            foreach (var comp in comps)
            {
                comp.Notify_ThingDespawned(thing);
            }
        }

        public void Notify_PawnEnteredRoom(Pawn pawn)
        {
            foreach (var comp in comps)
            {
                comp.Notify_PawnEnteredRoom(pawn);
            }
        }

        public void Notify_PawnLeftRoom(Pawn pawn)
        {
            foreach (var comp in comps)
            {
                comp.Notify_PawnLeftRoom(pawn);
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
            RegenerateData();
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

        public void RegenerateData()
        {
            var roomCells = Room.Cells.ToArray();
            int minX, minZ = minX = int.MaxValue;
            int maxX, maxZ = maxX = int.MinValue;
            for (int i = 0; i < roomCells.Length; i++)
            {
                var cell = roomCells[i];
                if (minX > cell.x)
                {
                    minX = cell.x;
                    cornerCells[0] = cell;
                }
                if (maxX < cell.x)
                {
                    maxX = cell.x;
                    cornerCells[1] = cell;
                }
                if (minZ > cell.z)
                {
                    minZ = cell.z;
                    cornerCells[2] = cell;
                }
                if (maxZ < cell.z)
                {
                    maxZ = cell.z;
                    cornerCells[3] = cell;
                }
            }
            minVec = new IntVec3(minX, 0, minZ);
            size = new IntVec2(maxX - minX + 1, maxZ - minZ + 1);
            actualCenter = new Vector3(minX + (size.x / 2f), 0, minZ + (size.z / 2f));
            drawPos = new Vector3(minX, AltitudeLayer.FogOfWar.AltitudeFor(), minZ);

            //Get Roof and Border Cells
            GenerateCellData();
        }

        private void GenerateCellData()
        {
            var tCells = new HashSet<IntVec3>();
            var bCells = new HashSet<IntVec3>();
            foreach (IntVec3 c in Room.Cells)
            {
                if (!Map.roofGrid.RoofAt(c)?.isThickRoof ?? false)
                    tCells.Add(c);

                for (int i = 0; i < 4; i++)
                {
                    IntVec3 cardinal = c + GenAdj.CardinalDirections[i];

                    var region = cardinal.GetRegion(Map);
                    if ((region == null || region.Room != Room) && cardinal.InBounds(Map))
                    {
                        bCells.Add(cardinal);
                    }
                }
            }

            borderCells = bCells.ToArray();
            thinRoofCells = tCells.ToArray();
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
