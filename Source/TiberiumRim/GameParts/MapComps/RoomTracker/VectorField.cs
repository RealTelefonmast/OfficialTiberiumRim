using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class VectorField
    {
        /*
        private Map map;

        public Dictionary<Vector3, Vector2> field = new Dictionary<Vector3, Vector2>();

        public IntVec3 origin;
        public Vector3 originV3;

        public VectorField(Map map)
        {
            this.map = map;
        }


        public Texture2D GetTextureFor(RoomComponent_Atmospheric comp)
        {
            var flowRadius = TiberiumContent.FlowMapRadius;
            var pixelDensity = TiberiumContent.FlowMapPixelDensity;
            var pixelWidth = pixelDensity * comp.Parent.Size.x;
            var pixelHeight = pixelDensity * comp.Parent.Size.z;

            var pxPart = 1f / pixelDensity;

            Color[] colors = new Color[pixelHeight * pixelWidth];
            for (int p = 0; p < colors.Length; p++)
            {
                colors[p] = Color.clear;
            }

            //Get Map Canvas
            var canvas = StaticData.PixelCanvas(map.Size.x, pixelDensity);
            var mapPixelSize = map.Size.x * pixelDensity;

            Log.Message($"Colors Size: {comp.Parent.Size} | {pixelWidth},{pixelHeight} | {colors.Length} should be {pixelWidth * pixelHeight}");
            Log.Message($"Canvas Size: {map.Size.x} | {mapPixelSize} | {canvas.Length} should be {mapPixelSize * mapPixelSize}");
            foreach (var cell in comp.Room.Cells)
            {
                var diffCell = cell - comp.Parent.MinVec;
                var vec3 = cell.ToVector3();
                for (int x = 0; x < pixelDensity; x++)
                {
                    for (int z = 0; z < pixelDensity; z++)
                    {
                        Vector3 vec3Pixel = new Vector3(vec3.x + (pxPart + (x * pxPart)), 0, vec3.z + (pxPart + (z * pxPart)));

                        int pixelX = (diffCell.x) * pixelDensity + x;
                        int pixelZ = (diffCell.z) * pixelDensity + z;
                        int index = pixelX + (pixelZ * pixelWidth);
                        try
                        {
                            colors[index] = ColorAt(cell, vec3Pixel, x, z, comp.Connections);
                        }
                        catch (IndexOutOfRangeException exc)
                        {
                            Log.Message($"Colors[] Broke! At {cell}|{diffCell} with index {index}");
                            throw exc;
                        }

                        //Set Map Canvas Pixel
                        int pixelX2 = (cell.x) * pixelDensity + x;
                        int pixelZ2 = (cell.z) * pixelDensity + z;
                        int index2 = pixelX2 + (pixelZ2 * mapPixelSize);
                        try
                        {
                            canvas[index2] = colors[index];
                        }
                        catch (IndexOutOfRangeException exc)
                        {
                            Log.Message($"Cell: {cell} | {pixelX2},{pixelZ2} | Index: {index2}", true);
                            throw exc;
                        }
                    }
                }
            }

            //Write to RT
            var rt = StaticData.FlowMapTextureFor(map.uniqueID);

            RenderTexture.active = rt;
            Texture2D renderTextureAddition = new Texture2D(mapPixelSize, mapPixelSize, TextureFormat.RGBAFloat, false);
            renderTextureAddition.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);

            for (var i = 0; i < canvas.Length; i++)
            {
                var color = canvas[i];
                if (color.a <= 0) continue;
                int newX = i % renderTextureAddition.width;
                int newY = i / renderTextureAddition.width;
                renderTextureAddition.SetPixel(newX, newY, color);
            }

            Graphics.Blit(renderTextureAddition, rt);
            RenderTexture.active = null;

            byte[] bytes = renderTextureAddition.EncodeToPNG();
            var dirPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/TestFolder/";
            File.WriteAllBytes(dirPath + "MapTex.png", bytes);

            return renderTextureAddition;
            TiberiumContent.GenerateTextureFrom(canvas, new IntVec2(mapPixelSize, mapPixelSize), "RoomTextureCANVASBIG " + comp.Room.ID);
            return TiberiumContent.GenerateTextureFrom(colors, new IntVec2(pixelWidth, pixelHeight), "RoomTexture " + comp.Room.ID);
        }

        private Color ColorAt(IntVec3 cell, Vector3 pixelVec, int PX, int PZ, List<AtmosphericConnector> conns)
        {
            Color color = new Color();//new Color(1,1,1,1);
            float r = 0, g = 0;
            int intersections = 0;
            var pixelInRange = false;
            foreach (var conn in conns)
            {
                var pos = conn.Building.Position.ToVector3();
                var dist = Mathf.Abs(Vector3.Distance(pos, pixelVec));
                if (dist > TiberiumContent.FlowMapRadius) continue;
                pixelInRange = true;
                var intVecDiff = cell - conn.Building.Position;
                var rot = conn.RotationFrom(intVecDiff);
                var flowMap = FlowMapForConn(conn, rot, out var size);
                if (flowMap == null) continue;

                color += ColorFrom(flowMap, PX, PZ, conn.Building.Position, cell, size, rot);
                intersections++;
                //r += color2.r;
                //g += color2.g;
            }

            if (!pixelInRange)
            {
                color = new Color(0.5f, 0.5f, 0, 1);
            }
            else
            {
                color /= intersections;
                //r /= conns.Count;
                //g /= conns.Count;
                //color = new Color(r, g, 0, 1);
            }

            return color;
        }

        private Color ColorFrom(Color[] flowMap, int PX, int PZ, IntVec3 sourceVec, IntVec3 curVec, IntVec2 size, Rot4 rot)
        {
            var PD = TiberiumContent.FlowMapPixelDensity;
            var size2 = size / PD;
            IntVec3 startVec = StartVec(sourceVec, size2, rot);
            IntVec3 diff = curVec - startVec;
            int pixelX = diff.x * PD + PX;
            int pixelZ = diff.z * PD + PZ;
            int index = pixelX + (pixelZ * size.x);
            if (index < 0 || index > flowMap.Length) return Color.blue;//new Color(0.5f, 0.5f, 0);
            return flowMap[index];
        }

        private IntVec3 StartVec(IntVec3 sourceVec, IntVec2 size, Rot4 rot)
        {
            var dens = TiberiumContent.FlowMapPixelDensity;
            if (rot == Rot4.North)
            {
                return new IntVec3(sourceVec.x - (size.x / 2), 0, sourceVec.z);
            }
            else if (rot == Rot4.South)
            {
                return new IntVec3(sourceVec.x - (size.x / 2), 0, sourceVec.z - size.z);
            }
            else if (rot == Rot4.East)
            {
                return new IntVec3(sourceVec.x, 0, sourceVec.z - (size.z / 2));
            }
            return new IntVec3(sourceVec.x - size.x, 0, sourceVec.z - (size.z / 2));
        }

        private Color[] FlowMapForConn(AtmosphericConnector conn, Rot4 rot, out IntVec2 size)
        {
            //Log.Message("Getting Map From: " + conn.Building + " Rot: " + rot.ToStringWord() + " IsFlowing? " + conn.IsFlowing + " FlowDir: " + conn.FlowDirection.ToStringWord());
            size = IntVec2.Invalid;
            if (!conn.IsFlowing) return null;
            if (conn.FlowDirection == Rot4.North)
            {
                size = TiberiumContent.FlowMapSize;
                if (rot == Rot4.South)
                {
                    return TiberiumContent.FlowMapColorsSouth_Inverted;
                }
                return TiberiumContent.FlowMapColorsNorth;
            }
            if (conn.FlowDirection == Rot4.East)
            {
                size = TiberiumContent.FlowMapSizeRotated;
                if (rot == Rot4.West)
                {
                    return TiberiumContent.FlowMapColorsWest_Inverted;
                }
                return TiberiumContent.FlowMapColorsEast;
            }
            if (conn.FlowDirection == Rot4.South)
            {
                size = TiberiumContent.FlowMapSize;
                if (rot == Rot4.North)
                {
                    return TiberiumContent.FlowMapColorsNorth_Inverted;
                }
                return TiberiumContent.FlowMapColorsSouth;
            }
            size = TiberiumContent.FlowMapSizeRotated;
            if (rot == Rot4.East)
            {
                return TiberiumContent.FlowMapColorsEast_Inverted;
            }
            return TiberiumContent.FlowMapColorsWest;
        }

        private Color[] FlowMapForRot(Rot4 rot, out IntVec2 size)
        {
            Log.Message("Getting Flow Map For: " + rot.ToStringWord());
            if (rot == Rot4.North)
            {
                size = TiberiumContent.FlowMapSize;
                return TiberiumContent.FlowMapColorsSouth_Inverted;
            }
            else if (rot == Rot4.East)
            {
                size = TiberiumContent.FlowMapSizeRotated;
                return TiberiumContent.FlowMapColorsWest_Inverted;
            }
            else if (rot == Rot4.South)
            {
                size = TiberiumContent.FlowMapSize;
                return TiberiumContent.FlowMapColorsNorth_Inverted;
            }
            size = TiberiumContent.FlowMapSizeRotated;
            return TiberiumContent.FlowMapColorsEast_Inverted;
        }

        public void DrawVectors()
        {
            if (field.EnumerableNullOrEmpty()) return;
            foreach (var entry in field)
            {
                Matrix4x4 matrix = default;
                var v = entry.Value;
                var angle = (Mathf.Atan2(v.x, v.y) * (180f / Mathf.PI));
                var drawPos = entry.Key + new Vector3(0.5f, AltitudeLayer.FogOfWar.AltitudeFor(), 0.5f);
                matrix.SetTRS(drawPos, angle.ToQuat(), Vector3.one);
                var mat = MaterialPool.MatFrom(TiberiumContent.VectorArrow);
                Graphics.DrawMesh(MeshPool.plane10, matrix, mat, 0);
            }
        }
        */
    }
    
}
