using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class RadiationInfectionGrid : IExposable
    {
        //General
        private Map map;
        private int mapCells;
        private BoolGrid affectedCells;

        //Infection
        private int[] infectionIntGrid;
        private byte[] infectionBytes;

        //Radiation
        private int[] radiationIntGrid;
        private byte[] radiationBytes;
        private List<IRadiationSource> RadiationSources = new List<IRadiationSource>();
        public BoolGrid radiationCells;

        private CellBoolDrawer drawerInt;

        public CellBoolDrawer RadiationDrawer => drawerInt ??= new CellBoolDrawer(RadiationDrawerBool, RadiationDrawerColor, RadiationDrawerExtraColor, map.Size.x, map.Size.z, 3605, 0.25f);

        private bool RadiationDrawerBool(int index)
        {
            IntVec3 intVec = CellIndicesUtility.IndexToCell(index, map.Size.x);
            bool passed = !intVec.Fogged(map) && RadiationAt(intVec) > 0f;
            return passed;
        }

        private Color RadiationDrawerColor()
        {
            return Color.white;
        }

        private Color RadiationDrawerExtraColor(int index)
        {
            IntVec3 intVec = CellIndicesUtility.IndexToCell(index, map.Size.x);
            float num = RadiationAt(intVec);
            Color color = Color.Lerp(Color.green, Color.red, num);
            if (num > 1)
            {
                //Log.Message("num: " + num + " inv lerp: " + Mathf.InverseLerp(0, 2, num));
                color = Color.Lerp(Color.red, new Color(0.5f,0,1), Mathf.InverseLerp(1, 3, num));
            }
            return color;
        }

        public RadiationInfectionGrid(Map map)
        {
            this.map = map;
            this.mapCells = map.cellIndices.NumGridCells;
            affectedCells = new BoolGrid(map);
            radiationCells = new BoolGrid(map);

            radiationIntGrid = new int[mapCells];
            infectionIntGrid = new int[mapCells]; 
            radiationBytes = new byte[mapCells * 4];
            infectionBytes = new byte[mapCells * 4];

            //radiationGrid = new ByteGrid(map);
            //infectionGrid = new ByteGrid(map);
        }

        //TODO: Check if exposing necessary
        public void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                Buffer.BlockCopy(radiationIntGrid, 0, radiationBytes, 0, mapCells * 4);
                Buffer.BlockCopy(infectionIntGrid, 0, infectionBytes, 0, mapCells * 4);
            }

            Scribe_Deep.Look(ref affectedCells, "affectedHediffCells");
            Scribe_Values.Look(ref mapCells, "mapCells");
            DataExposeUtility.ByteArray(ref radiationBytes, "radiationBytes");
            DataExposeUtility.ByteArray(ref infectionBytes, "infectionBytes");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                Buffer.BlockCopy(radiationBytes, 0, radiationIntGrid, 0, mapCells);
                Buffer.BlockCopy(infectionBytes, 0, infectionIntGrid, 0, mapCells);
            }
        }

        public void Update()
        {
            if (TRUtils.Tiberium().GameSettings.RadiationOverlay)
            {
                RadiationDrawer.MarkForDraw();
            }
            RadiationDrawer.CellBoolDrawerUpdate();
        }

        //General
        private int Index(IntVec3 pos)
        {
            return map.cellIndices.CellToIndex(pos);
        }

        public bool IsAffected(IntVec3 pos)
        {
            return affectedCells[pos];
        }

        public void UpdateAffected(IntVec3 pos)
        {
            affectedCells[pos] = radiationIntGrid[Index(pos)] > 0 || infectionIntGrid[Index(pos)] > 0;
        }

        //Radiation
        public List<IRadiationSource> RadiationSourcesAt(IntVec3 pos)
        {
            return RadiationSources.Where(r => r.AffectsCell(pos)).ToList();
        }

        public void Notify_SourceSpawned(IRadiationSource source)
        {
            RadiationSources.Add(source);
            foreach (var cell in source.AffectedCells)
            {
                if(!cell.InBounds(map)) continue;
                radiationCells[cell] = true;
            }
        }

        public void Notify_SourceDespawned(IRadiationSource source)
        {
            RadiationSources.Remove(source);
            foreach (var cell in source.AffectedCells)
            {
                if (!cell.InBounds(map) || RadiationSources.Any(s => s != source && s.AffectsCell(cell))) continue;
                radiationCells[cell] = false;
            }
        }

        public bool IsInRadiationSourceRange(IntVec3 pos)
        {
            return radiationCells[pos];
        }

        public float RadiationAt(IntVec3 pos)
        {
            return TRUtils.InverseLerpUnclamped(0, 255, (float)RadiationIntAt(pos));
        }

        private int RadiationIntAt(IntVec3 pos)
        {
            return radiationIntGrid[Index(pos)];
        }

        public void AddRadiation(IntVec3 pos, float pct)
        {
            var radAt = (int)RadiationIntAt(pos);
            var radExtra = (int)Mathf.LerpUnclamped(0, 255, pct);
            SetRadInt(pos, radAt + radExtra);
            RadiationDrawer.SetDirty();
        }

        public void SetRadInt(IntVec3 pos, int value)
        {
            radiationIntGrid[Index(pos)] = Mathf.Clamp(value, 0, int.MaxValue);
            UpdateAffected(pos);
        }

        //Infection
        public float InfectionAt(IntVec3 pos)
        {
            return TRUtils.InverseLerpUnclamped(0, 255, (float)InfectionIntAt(pos));
        }

        private int InfectionIntAt(IntVec3 pos)
        {
            return infectionIntGrid[Index(pos)];
        }

        public void AddInfection(IntVec3 pos, float pct)
        {
            var infAt = (int)InfectionIntAt(pos);
            var infExtra = (int)Mathf.LerpUnclamped(0, 255, pct);
            SetInfInt(pos, infAt + infExtra); //(int)Mathf.Clamp(infAt + infExtra, 0, 255));
        }

        private void SetInfInt(IntVec3 pos, int value)
        {
            infectionIntGrid[Index(pos)] = value;
            UpdateAffected(pos);
        }

        public void DrawValues()
        {
            if (Find.CameraDriver.CurrentZoom == CameraZoomRange.Closest)
            {
                for (int i = 0; i < mapCells; i++)
                {
                    if (!affectedCells[i]) continue;
                    Vector3 v = GenMapUI.LabelDrawPosFor(map.cellIndices.IndexToCell(i));
                    Vector3 v2 = v + new Vector3(0, 15f, 0);

                    GenMapUI.DrawThingLabel(v2, radiationIntGrid[i].ToString(),RadiationDrawerExtraColor(i));
                    GenMapUI.DrawThingLabel(v, infectionIntGrid[i].ToString(),
                        Color.Lerp(Color.white, Color.cyan, InfectionAt(map.cellIndices.IndexToCell(i))));
                    //GenMapUI.DrawThingLabel(v2, Container.StoredPercent.ToString() + "p", Color.yellow);
                }
            }
        }
    }
}
