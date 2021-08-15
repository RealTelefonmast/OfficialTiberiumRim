﻿using System;
using System.IO;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    [StaticConstructorOnStartup]
    public class TiberiumContent
    {
        static TiberiumContent()
        {
            PreGenerateFlowMapVectors();
        }

        private static int Compare(IntVec3 a, IntVec3 b, int gridWidth)
        {
            var aValue = a.x + (a.z * gridWidth);
            var bValue = b.x + (b.z * gridWidth);
            return aValue - bValue;
        }

        public static void PreGenerateFlowMapVectors()
        {
            var radius = FlowMapRadius;
            var pixelDensity = FlowMapPixelDensity;

            var origin = new IntVec3(radius, 0, radius);
            var originZero = Vector3.zero;
            var intSize = new IntVec2(radius * 2 + 1, radius * 2 + 1);

            var size = (int)pixelDensity * intSize.x;
            //var width = (int)pixelDensity * intSize.x;
            //var height = (int)pixelDensity * intSize.z;

            Color[] pixels = new Color[size * size];
            Color[] pixels2 = new Color[size * size];
            for (int p = 0; p < pixels.Length; p++)
            {
                pixels[p] = pixels2[p] = new Color(0.5f, 0.5f, 0, 1);
            }

            var cells = new CellRect(0,0, intSize.x, intSize.z); //GenRadial.RadialCellsAround(origin, radius, true); //TRUtils.SectorCells(origin, null, radius, 360, 0, true);

            foreach (var cell in cells)
            {
                var originVec3 = origin.ToVector3Shifted();
                var cellFlow = (cell.ToVector3() - originVec3);
                var cellFlowInv = (originVec3 - cell.ToVector3());
                FillFlowMapColors(ref pixels, pixelDensity, radius, originZero, cellFlow, cell, size);
                FillFlowMapColors(ref pixels2, pixelDensity, radius, originZero, cellFlowInv, cell, size, true);
            }

            int half = size / 2;
            FlowMapSize = new IntVec2(size, half);
            FlowMapSizeRotated = new IntVec2(half, size);

            FlowMapColorsNorth = GetSection(pixels, 0, half, size, half, size);
            FlowMapColorsEast  = GetSection(pixels, half, 0, half, size, size);
            FlowMapColorsSouth = GetSection(pixels, 0, 0, size, half, size);
            FlowMapColorsWest  = GetSection(pixels, 0, 0, half, size, size);

            FlowMapColorsNorth_Inverted = GetSection(pixels2, 0, 0, size, half, size);
            FlowMapColorsEast_Inverted  = GetSection(pixels2, 0, 0, half, size, size);
            FlowMapColorsSouth_Inverted = GetSection(pixels2, 0, half, size, half, size);
            FlowMapColorsWest_Inverted = GetSection(pixels2, half, 0, half, size, size);

            GenerateTextureFrom(pixels,  new IntVec2(size, size), "TestFlowMapFull");
            GenerateTextureFrom(pixels2, new IntVec2(size, size), "TestFlowMapFull_Inv");
            GenerateTextureFrom(FlowMapColorsNorth, FlowMapSize, "TestFlowMapNorth");
            GenerateTextureFrom(FlowMapColorsEast,  FlowMapSizeRotated, "TestFlowMapEast");
            GenerateTextureFrom(FlowMapColorsSouth, FlowMapSize, "TestFlowMapSouth");
            GenerateTextureFrom(FlowMapColorsWest,  FlowMapSizeRotated, "TestFlowMapWest");

            GenerateTextureFrom(FlowMapColorsNorth_Inverted, FlowMapSize, "TestFlowMapNorth_Inverted");
            GenerateTextureFrom(FlowMapColorsEast_Inverted,  FlowMapSizeRotated, "TestFlowMapEast_Inverted");
            GenerateTextureFrom(FlowMapColorsSouth_Inverted, FlowMapSize, "TestFlowMapSouth_Inverted");
            GenerateTextureFrom(FlowMapColorsWest_Inverted,  FlowMapSizeRotated, "TestFlowMapWest_Inverted");
        }

        private static void FillFlowMapColors(ref Color[] pixels, int pixelDensity, float radius, Vector3 origin, Vector3 cellFlow, IntVec3 cell, int size, bool inv = false)
        {
            for (int x = 0; x < pixelDensity; x++)
            {
                for (int z = 0; z < pixelDensity; z++)
                {

                    //Split cell vector into smaller vector bits per pixel
                    var partX = (inv ? -1f : 1f) / pixelDensity;
                    var partZ = (inv ? -1f : 1f) / pixelDensity;
                    Vector3 pixelFlow = new Vector3(cellFlow.x + (partX + (x * partX)), 0, cellFlow.z + (partZ + (z * partZ)));

                    //Create the flow vector and normalize
                    Vector2 vec = new Vector2(pixelFlow.x, pixelFlow.z);
                    vec.Normalize();

                    //Get the gradual decay the further away we are from the source
                    var dist = Mathf.Abs(Vector3.Distance(origin, pixelFlow));
                    var decay = Mathf.InverseLerp((float)radius, 0f, dist);
                    vec *= decay;

                    //Map the vector onto the RG values of the flow color
                    float r = Mathf.InverseLerp(-1, 1, vec.x);
                    float g = Mathf.InverseLerp(-1, 1, vec.y);

                    //Get the pixel index and set the color
                    int pixelX = cell.x * (int)pixelDensity + x;
                    int pixelZ = cell.z * (int)pixelDensity + z;
                    int index = (pixelX + (pixelZ * size));
                    pixels[index] = new Color(r, g, 0, 1);
                }
            }
        }

        public static Color[] GetSection(Color[] pixels, int startX, int startY, int width, int height, int oldSize)
        {
            Color[] newPixels = new Color[width * height];
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    int oldIndex = startX + x + ((startY + z) * oldSize);
                    int newIndex = x + (z * width);
                    newPixels[newIndex] = pixels[oldIndex];
                }
            }
            return newPixels;
        }

        public static Texture2D GenerateTextureFrom(Color[] pixels, IntVec2 size, string name)
        {
            Texture2D newTex = new Texture2D(size.x, size.z, TextureFormat.RGBAFloat, false);
            newTex.SetPixels(pixels);
            newTex.wrapMode = TextureWrapMode.Clamp;
            newTex.Apply();

            var dirPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/TestFolder/";
            byte[] bytes = newTex.EncodeToPNG();
            File.WriteAllBytes(dirPath + name + ".png", bytes);
            return newTex;
        }

        private static Color[] Invert(Color[] pixels, IntVec2 size, bool leftRight = true)
        {
            Color[] newPixels = new Color[pixels.Length];
            for (int x = 0; x < size.x; x++)
            {
                for (int z = 0; z < size.z; z++)
                {
                    if (leftRight)
                        newPixels[x + z * size.x] = pixels[size.x - 1 - x + (size.z - 1 - z) * size.x];
                    else
                        newPixels[x + z * size.x] = pixels[size.x - 1 - x + (size.z - 1 - z) * size.x];
                }
            }
            return newPixels;
        }

        private static Color[] RotateRight(Color[] pixels, IntVec2 size, out IntVec2 newSize, bool invert = false)
        {
            Color[] newPixels = new Color[pixels.Length];
            for (int x = 0; x < size.x; x++)
            {
                for (int z = 0; z < size.z; z++)
                {
                    int colorIndex = ((size.x-1) - x) + (z * (int)size.x);
                    int transposedIndex = z + (x * (int)size.z); //TRANSPOSING!

                    var col = pixels[colorIndex];
                    newPixels[transposedIndex] = invert ? new Color(col.g, col.r, 0, 1) : col;
                }
            }
            newSize = new IntVec2(size.z, size.x);
            return newPixels;
        }

        private static Color[] RotateLeft(Color[] pixels, IntVec2 size, out IntVec2 newSize)
        {
            Color[] newPixels = new Color[pixels.Length];
            for (int x = 0; x < size.x; x++)
            {
                for (int z = 0; z < size.z; z++)
                {
                    int colorIndex = x + (((size.z-1) - z) * (int)size.x);
                    int transposedIndex = z + (x * (int)size.z); //TRANSPOSING!

                    newPixels[transposedIndex] = pixels[colorIndex];
                }
            }
            newSize = new IntVec2(size.z, size.x);
            return newPixels;
        }

        public static void LoadFromMainAssetPack(AssetBundle bundle)
        {
            FlowMapOnBlend = bundle.LoadAsset<Shader>("FlowMapOnBlend");
            TextureBlend = bundle.LoadAsset<Shader>("TextureBlend");
            FlowMapShader = bundle.LoadAsset<Shader>("FlowMapShader");
        }

        public static Graphic_LinkedNetworkStructure ConnectionGraphicForNetwork(NetworkType type)
        {
            return type switch
            {
                NetworkType.TiberiumProcessing => TiberiumNetworkPipes,
                NetworkType.GasPiping => null,
                _ => null
            };
        }

        public static GraphicLinkedNetworkStructureOverlay OverlayGraphicForNetwork(NetworkType type)
        {
            return type switch
            {
                NetworkType.TiberiumProcessing => TiberiumNetworkPipesOverlay,
                NetworkType.GasPiping => null,
                _ => null
            };
        }

        public static readonly GraphicLinkedNetworkStructureOverlay TiberiumNetworkPipesOverlay = new GraphicLinkedNetworkStructureOverlay(GraphicDatabase.Get<Graphic_Single>("Buildings/Common/Network/TNW_PipeOverlayAtlas", ShaderDatabase.Transparent, Vector2.one, new ColorInt(155, 255, 0).ToColor));
        public static readonly GraphicLinkedNetworkStructureOverlay TiberiumNetworkPipesGlow = new GraphicLinkedNetworkStructureOverlay(GraphicDatabase.Get<Graphic_Single>("Buildings/Common/Network/TNW_PipeOverlayAtlas", ShaderDatabase.MoteGlow, Vector2.one, Color.white));
        public static readonly Graphic_LinkedNetworkStructure TiberiumNetworkPipes = new Graphic_LinkedNetworkStructure(GraphicDatabase.Get<Graphic_Single>("Buildings/Common/Network/TNW_PipeAtlas", ShaderDatabase.Transparent, Vector2.one, Color.white));

        public static readonly int FlowMapRadius = 5;
        public static readonly int FlowMapPixelDensity = 16;
        public static IntVec2 FlowMapSize;
        public static IntVec2 FlowMapSizeRotated;
        //public static Color[] FlowMapColorsFull;

        public static Color[] FlowMapColorsNorth;
        public static Color[] FlowMapColorsEast;
        public static Color[] FlowMapColorsSouth;
        public static Color[] FlowMapColorsWest;

        public static Color[] FlowMapColorsNorth_Inverted;
        public static Color[] FlowMapColorsEast_Inverted;
        public static Color[] FlowMapColorsSouth_Inverted;
        public static Color[] FlowMapColorsWest_Inverted;

        //Shader Stuff
        //Imported
        public static Shader AlphaShader;
        public static Material AlphaShaderMaterial;

        public static Shader FlowMapOnBlend;
        public static Shader TextureBlend;
        public static Shader FlowMapShader;

        public static readonly Texture2D Nebula1 = ContentFinder<Texture2D>.Get("ShaderMaterials/Nebula1", false);
        public static readonly Texture2D Nebula2 = ContentFinder<Texture2D>.Get("ShaderMaterials/Nebula2", false);

        //Misc
        public static readonly Texture2D VectorArrow = ContentFinder<Texture2D>.Get("Misc/Arrow", false);

        //Icons
        public static readonly Texture2D MissingConnection = ContentFinder<Texture2D>.Get("UI/Icons/TiberiumNetwork/ConnectionMissing", false);
        public static readonly Texture2D MarkedForDeath = ContentFinder<Texture2D>.Get("UI/Icons/Marked", false);
        public static readonly Texture2D Icon_EVA = ContentFinder<Texture2D>.Get("UI/Icons/EVA", false);

        //Turrets
        public static readonly Material TurretCable = MaterialPool.MatFrom("Buildings/Nod/Defense/Turrets/TurretCable");

        //Internal RW Crap /FROM TexButton
        public static readonly Texture2D DeleteX = ContentFinder<Texture2D>.Get("UI/Buttons/Delete", true);
        public static readonly Texture2D Plus = ContentFinder<Texture2D>.Get("UI/Buttons/Plus", true);
        public static readonly Texture2D Minus = ContentFinder<Texture2D>.Get("UI/Buttons/Minus", true);
        public static readonly Texture2D Infinity = ContentFinder<Texture2D>.Get("UI/Buttons/Infinity", true);
        public static readonly Texture2D Suspend = ContentFinder<Texture2D>.Get("UI/Buttons/Suspend", true);
        public static readonly Texture2D Copy = ContentFinder<Texture2D>.Get("UI/Buttons/Copy", true);
        public static readonly Texture2D Paste = ContentFinder<Texture2D>.Get("UI/Buttons/Paste", true);

        //UI - Menus
        public static readonly Texture2D BGPlanet = ContentFinder<Texture2D>.Get("UI/Menu/Background", true);
        public static readonly Texture2D ResearchBG = ContentFinder<Texture2D>.Get("UI/Menu/ResearchBG", true);
        public static readonly Texture2D MainMenu = ContentFinder<Texture2D>.Get("UI/Menu/MainMenu", true);
        public static readonly Texture2D MenuWindow = ContentFinder<Texture2D>.Get("UI/Menu/MenuWindow", true);
        public static readonly Texture2D Banner = ContentFinder<Texture2D>.Get("UI/Menu/Banner", true);
        public static readonly Texture2D LockedBanner = ContentFinder<Texture2D>.Get("UI/Menu/LockedBanner", true);

        public static readonly Texture2D Undiscovered = ContentFinder<Texture2D>.Get("UI/Menu/Undiscovered", true);
        public static readonly Texture2D Fact_Undisc = ContentFinder<Texture2D>.Get("UI/Menu/Fact_Undiscovered", true);
        public static readonly Texture2D Des_Undisc = ContentFinder<Texture2D>.Get("UI/Menu/Des_Undiscovered", true);
        public static readonly Texture2D Tab_Undisc = ContentFinder<Texture2D>.Get("UI/Menu/Tab_Undiscovered", true);
        public static readonly Texture2D InfoButton = ContentFinder<Texture2D>.Get("UI/Buttons/InfoButton", true);
        public static readonly Texture2D SideBarArrow = ContentFinder<Texture2D>.Get("UI/Menu/Arrow", true);

        public static readonly Texture2D HighlightAtlas = ContentFinder<Texture2D>.Get("UI/Widgets/TutorHighlightAtlas", true);

        public static readonly Texture2D AttentionMarker = ContentFinder<Texture2D>.Get("UI/Icons/AttentionMarker", true);
        public static readonly Texture2D NewMarker = ContentFinder<Texture2D>.Get("UI/Icons/NewMarker", true);

        //public static readonly Texture2D HightlightInMenu = ContentFinder<Texture2D>.Get("UI/Icons/HighLight", true);
        public static readonly Texture2D OpenMenu = ContentFinder<Texture2D>.Get("UI/Icons/OpenMenu", true);
        public static readonly Texture2D Construct = ContentFinder<Texture2D>.Get("UI/Icons/Construct", true);
        public static readonly Texture2D SelectThing = ContentFinder<Texture2D>.Get("UI/Icons/SelectThing", true);
        //UI - Icons
        //--Controls
        //----Harvester
        public static readonly Texture2D HarvesterRefinery = ContentFinder<Texture2D>.Get("UI/Icons/Controls/Harvester/NewRefinery", true);
        public static readonly Texture2D HarvesterReturn = ContentFinder<Texture2D>.Get("UI/Icons/Controls/Harvester/Return", true);
        public static readonly Texture2D HarvesterHarvest = ContentFinder<Texture2D>.Get("UI/Icons/Controls/Harvester/Harvest", true);
        public static readonly Texture2D HarvesterValue = ContentFinder<Texture2D>.Get("UI/Icons/Controls/Harvester/Value", true);
        public static readonly Texture2D HarvesterNearest = ContentFinder<Texture2D>.Get("UI/Icons/Controls/Harvester/Nearest", true);
        public static readonly Texture2D HarvesterMoss = ContentFinder<Texture2D>.Get("UI/Icons/Controls/Harvester/Moss", true);

        //----SuperWeapon
        public static readonly Texture2D NodNukeIcon = ContentFinder<Texture2D>.Get("UI/Icons/Controls/SuperWep/Launch_Nuke", true);
        public static readonly Texture2D IonCannonIcon = ContentFinder<Texture2D>.Get("UI/Icons/Controls/SuperWep/Launch_IonCannon", true);
        public static readonly Texture2D FireStorm_On = ContentFinder<Texture2D>.Get("UI/Icons/Controls/SuperWep/Firestorm_On", true);
        public static readonly Texture2D FireStorm_Off = ContentFinder<Texture2D>.Get("UI/Icons/Controls/SuperWep/Firestorm_Off", true);

        //Harvester Bar
        public static readonly Material Harvester_EmptyBar = SolidColorMaterials.NewSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f, 0.65f), ShaderDatabase.MetaOverlay);
        public static readonly Material Harvester_FilledBar = SolidColorMaterials.NewSolidColorMaterial(new Color(0f, 1f, 1f, 1f), ShaderDatabase.MetaOverlay);

        public static readonly Material RedMaterial = SolidColorMaterials.NewSolidColorMaterial(new Color(1f, 0f, 0f, 1f), ShaderDatabase.MetaOverlay);
        public static readonly Material GreenMaterial = SolidColorMaterials.NewSolidColorMaterial(new Color(0f, 1f, 0f, 1f), ShaderDatabase.MetaOverlay);
        public static readonly Material BlueMaterial = SolidColorMaterials.NewSolidColorMaterial(new Color(0f, 0f, 1f, 1f), ShaderDatabase.MetaOverlay);
        public static readonly Material ClearMaterial = SolidColorMaterials.NewSolidColorMaterial(new Color(0f, 0f, 0f, 0f), ShaderDatabase.MetaOverlay);

        //----Tib Container
        public static readonly Texture2D ContainMode_Sludge = ContentFinder<Texture2D>.Get("UI/Icons/Controls/TibContainer/ContainMode_Sludge", true);
        public static readonly Texture2D ContainMode_TripleSwitch = ContentFinder<Texture2D>.Get("UI/Icons/Controls/TibContainer/ContainMode_Storage", true);

        //--Faction Icons
        public static readonly Texture2D CommonIcon = ContentFinder<Texture2D>.Get("UI/Icons/Factions/Common", true);
        public static readonly Texture2D ForgottenIcon = ContentFinder<Texture2D>.Get("UI/Icons/Factions/Forgotten", true);
        public static readonly Texture2D GDIIcon = ContentFinder<Texture2D>.Get("UI/Icons/Factions/GDI", true);
        public static readonly Texture2D NodIcon = ContentFinder<Texture2D>.Get("UI/Icons/Factions/Nod", true);
        public static readonly Texture2D ScrinIcon = ContentFinder<Texture2D>.Get("UI/Icons/Factions/Scrin", true);
        public static readonly Texture2D BlackMarketIcon = ContentFinder<Texture2D>.Get("UI/Icons/Factions/BlackMarket", true);

        //--Tiberium Icons
        public static readonly Texture2D GreenTiberium = ContentFinder<Texture2D>.Get("Tiberium/Green/Tiberium_Green3");
        public static readonly Texture2D BlueTiberium = ContentFinder<Texture2D>.Get("Tiberium/Blue/Tiberium_blue3");
        public static readonly Texture2D RedTiberium = ContentFinder<Texture2D>.Get("Tiberium/Red/Tiberium_Red2");

        //--World
        public static readonly Material Infested_1 = MaterialPool.MatFrom("World/Tile/Tib_1", ShaderDatabase.WorldOverlayTransparentLit, 3505);
        public static readonly Material Infested_2 = MaterialPool.MatFrom("World/Tile/Tib_2", ShaderDatabase.WorldOverlayTransparentLit, 3505);
        public static readonly Material Infested_3 = MaterialPool.MatFrom("World/Tile/Tib_3", ShaderDatabase.WorldOverlayTransparentLit, 3505);
        public static readonly Material Infested_4 = MaterialPool.MatFrom("World/Tile/Tib_4", ShaderDatabase.WorldOverlayTransparentLit, 3505);

        public static readonly Material TibTile_1 = MaterialPool.MatFrom("World/Old/Tib_1", ShaderDatabase.WorldOverlayTransparentLit, 3505);
        public static readonly Material TibTile_2 = MaterialPool.MatFrom("World/Old/Tib_2", ShaderDatabase.WorldOverlayTransparentLit, 3505);
        public static readonly Material TibTile_3 = MaterialPool.MatFrom("World/Old/Tib_3", ShaderDatabase.WorldOverlayTransparentLit, 3505);
        public static readonly Material TibTile_4 = MaterialPool.MatFrom("World/Old/Tib_4", ShaderDatabase.WorldOverlayTransparentLit, 3505);
        public static readonly Material TibTile_Glacier = MaterialPool.MatFrom("World/Old/Tib_4", ShaderDatabase.WorldOverlayTransparentLit, 3505);

        //--Research
        public static readonly Texture2D Research_Active = ContentFinder<Texture2D>.Get("UI/Icons/Research/Active");
        public static readonly Texture2D Research_Available = ContentFinder<Texture2D>.Get("UI/Icons/Research/Available", true);

        //--Hediffs
        public static readonly Texture2D Hediff_Crystallizing = ContentFinder<Texture2D>.Get("UI/Icons/Hediffs/Crystallizing", true);
        public static readonly Texture2D Hediff_Mutation = ContentFinder<Texture2D>.Get("UI/Icons/Hediffs/Mutation", true);
        public static readonly Texture2D Hediff_Radiation = ContentFinder<Texture2D>.Get("UI/Icons/Hediffs/Radiation", true);
        public static readonly Texture2D Hediff_Immunity = ContentFinder<Texture2D>.Get("UI/Icons/Hediffs/Immunity", true);

        //ThingCategories
        public static readonly Texture2D TiberiumIcon = ContentFinder<Texture2D>.Get("UI/Icons/ThingCategories/TiberiumCategory", true);

        //Tiberium Network
        public static readonly Texture2D Network_MissingConnection = ContentFinder<Texture2D>.Get("UI/Icons/TiberiumNetwork/ConnectionMissing", true);

        //Targeter Mats
        public static readonly Material IonCannonTargeter = MaterialPool.MatFrom("UI/Targeters/Target_IonCannon", ShaderDatabase.Transparent);
        public static readonly Material NodNukeTargeter = MaterialPool.MatFrom("UI/Targeters/Target_Nuke", ShaderDatabase.Transparent);
        public static readonly Material ScrinLandingTargeter = MaterialPool.MatFrom("UI/Targeters/Target_IonCannon", ShaderDatabase.Transparent);

        public static readonly Material IonLightningMat = MaterialPool.MatFrom("Motes/LightningBoltIon", ShaderDatabase.MoteGlow);

        public static readonly Material ForcedTargetLineMat = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, new Color(1f, 0.5f, 0.5f));
    }
}