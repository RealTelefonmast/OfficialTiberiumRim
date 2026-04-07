using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using TeleCore.Defs.Extensions;
using TeleCore.Mod;
using TeleCore.Network;
using TeleCore.Network.IO;
using TeleCore.Static;
using TeleCore.ThingComps;
using TeleCore.ThingComps.Props;
using TeleCore.Utility;
using UnityEngine;
using Verse;

namespace TeleCore.Absorbed.RWLib.Patches;

internal static class RenderPatches
{
    //Equipment Size Fix
    //Draw Equipment Size Fix
    /*
    [HarmonyPatch(typeof(PawnRenderer)), HarmonyPatch(nameof(PawnRenderer.DrawEquipmentAiming))]
    public static class DrawEquipmentAimingPatch
    {
        static readonly MethodInfo injection = AccessTools.Method(typeof(DrawEquipmentAimingPatch), nameof(RenderInjection));

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var instructionList = instructions.ToList();
            for (var i = 0; i < instructionList.Count; i++)
            {
                var code = instructionList[i];
                var nextCode = i + 1 < instructionList.Count ? instructionList[i + 1] : null;
                if (nextCode != null && nextCode.opcode == OpCodes.Ret)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, injection);
                    continue;
                }
                yield return code;
            }
        }

        public static void RenderInjection(Mesh mesh, Vector3 drawLoc, Quaternion quat, Material mat, int layer, Thing thing)
        {
            var size = thing.Graphic.drawSize;
            Graphics.DrawMesh(mesh, Matrix4x4.TRS(drawLoc, quat, new Vector3(size.x, 1, size.y)), mat, layer);
        }
    }
    */

    /*
    //Fix pipe designator drawghost
    [HarmonyPatch(typeof(Designator_Place))]
    [HarmonyPatch(nameof(Designator_Place.DrawGhost))]
    internal static class Designator_Place_DrawGhost
    {
        public static bool Prefix(Designator_Place __instance, Color ghostCol)
        {
            if (__instance.PlacingDef is TRThingDef trDef)
            {
                if (trDef.building.blueprintGraphicData is { } data)
                {
                    IntVec3 center = UI.MouseCell();
                    Rot4 rot = __instance.placingRot;

                    Graphic graphic = GhostUtility.GhostGraphicFor(data.Graphic, trDef, ghostCol, __instance.StuffDef);
                    Vector3 loc = GenThing.TrueCenter(center, rot, trDef.Size, AltitudeLayer.Blueprint.AltitudeFor());
                    graphic.DrawFromDef(loc, rot, trDef, 0f);
                    return false;
                }
            }
            return true;
        }
    }
    */

    [HarmonyPatch(typeof(Designator_Build), MethodType.Constructor)]
    [HarmonyPatch(new Type[] { typeof(BuildableDef) })]
    internal static class Designator_Build_IconPatch
    {
        public static void Postfix(BuildableDef entDef, ref Designator_Build __instance)
        {
            var extension = entDef.GetModExtension<GraphicDataExtension>();
            if (extension != null)
            {
                if (extension.iconProportions.HasValue)
                    __instance.iconProportions = extension.iconProportions.Value;

                if (extension.iconDrawScale.HasValue)
                    __instance.iconDrawScale = extension.iconDrawScale.Value;
            }
        }
    }

    [HarmonyPatch(typeof(Graphic_Random), nameof(Graphic_Random.SubGraphicFor))]
    public static class SubGraphcForPatch
    {
        private static bool Prefix(Graphic_Random __instance, ref Graphic __result)
        {
            if (__instance is Graphic_RandomExtra extra)
            {
                if (Rand.Chance(extra.ParamRandChance))
                {
                    if (!UnityData.IsInMainThread)
                    {
                        __result = BaseContent.BadGraphic;
                        return false;
                    }
                    __result = TeleContent.ClearGraphic;
                    return false;
                }
            }

            return true;
        }
    }

    //
    //Fix projectile random graphics
    [HarmonyPatch(typeof(Thing), "Graphic", MethodType.Getter)]
    public static class ThingGraphicPatch
    {
        public static bool Prefix(Thing __instance, ref Graphic __result)
        {
            if (__instance is Projectile && TeleCoreMod.Settings.ProjectileGraphicRandomFix)
                if (__instance.DefaultGraphic is Graphic_Random Random)
                {
                    __result = Random.SubGraphicFor(__instance);
                    return false;
                }

            return true;
        }
    }

    //
    [HarmonyPatch(typeof(ThingWithComps))]
    [HarmonyPatch("Print")]
    public static class PrintPatch
    {
        public static bool Prefix(ThingWithComps __instance, SectionLayer layer)
        {
            var def = __instance.def;
            if (__instance is Blueprint b)
            {
                if (b.def.entityDefToBuild is TerrainDef)
                    return true;
                def = (ThingDef)b.def.entityDefToBuild;
            }

            if (def.HasFXExtension(out var extension))
            {
                TDrawing.Print(layer, __instance.Graphic, __instance, def, extension);
                return false;
            }

            return true;
        }
    }

    //TODO: Move to FlowCore
    [HarmonyPatch(typeof(Designator_Place))]
    [HarmonyPatch(nameof(Designator_Place.DrawMouseAttachments))]
    internal static class Designator_PlaceDrawMouseAttachmentsPatch
    {
        internal static readonly Dictionary<ThingDef, int> _indexes = new Dictionary<ThingDef, int>();

        public static void Postfix(Designator_Place __instance)
        {
            if (__instance.PlacingDef is ThingDef tDef)
            {
                var network = tDef.GetCompProperties<CompProperties_Network>();
                if (network != null && network.networks.Count > 1)
                {
                    var mousePos = Event.current.mousePosition + Designator_Place.PlaceMouseAttachmentDrawOffset;
                    var optionSize = 24;
                    var optionTotal = network.networks.Count * optionSize;
                    var rect = new Rect(mousePos - new Vector2(0, optionTotal), new Vector2(128, optionTotal));

                    TWidgets.DrawColoredBox(rect, TColor.White005, TColor.White025, 1);

                    float curY = 0f;
                    for (var i = 0; i < network.networks.Count; i++)
                    {
                        var part = network.networks[i];
                        var partRect = new Rect(rect.x, rect.y + curY, rect.width, optionSize);
                        DrawNetworkPartOption(partRect, tDef, part, i);
                        curY += optionSize;
                    }

                    //Scroll Change
                    var isScroll = Event.current.isScrollWheel;
                    var deltaDown = isScroll && Event.current.delta.y < 0;
                    var deltaUp = isScroll && Event.current.delta.y > 0;

                    if (isScroll)
                    {
                        var change = deltaDown ? -1 : deltaUp ? 1 : 0;
                        if (change != 0)
                        {
                            SetIndex(tDef, Mathf.Clamp(GetIndex(tDef) + change, 0, network.networks.Count - 1));
                            Event.current.Use();
                        }
                    }
                }
            }
        }

        private static void DrawNetworkPartOption(Rect rect, ThingDef def, NetworkPartConfig config, int index)
        {
            var isSelected = GetIndex(def) == index;

            if (isSelected)
                Widgets.DrawHighlight(rect);

            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect, config.networkDef.label);
            Text.Anchor = default;
        }

        internal static int GetIndex(ThingDef def)
        {
            return _indexes.TryGetValue(def, out var result) ? result : 0;
        }

        private static void SetIndex(ThingDef def, int index)
        {
            if (!_indexes.TryAdd(def, index))
            {
                _indexes[def] = index;
            }
        }
    }
    
//TODO: Move to FlowCore
    [HarmonyPatch(typeof(GenDraw))]
    [HarmonyPatch(nameof(GenDraw.DrawInteractionCells))]
    internal static class GenDrawDrawInteractionCellsPatch
    {
        public static void Postfix(ThingDef tDef, IntVec3 center, Rot4 placingRot)
        {
            var selThing = Find.Selector.SingleSelectedThing;
            var placing = Find.DesignatorManager.SelectedDesignator is Designator_Place;
            if (!placing && selThing != null && selThing.def == tDef && selThing.Spawned)
            {
                var comp = selThing.TryGetComp<CompNetwork>();
                if (comp != null)
                {
                    var selPart = comp.SelectedPart;
                    var partIO = selPart.PartIO;
                    foreach (var ioConn in partIO.Connections)
                    {
                        var pos = ioConn.Pos;
                        var drawPos = pos.Pos.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
                        DrawMode(drawPos, ioConn.Mode, pos.Dir);
                    }
                }
                return;
            }

            //Draw Network IO
            var network = tDef.GetCompProperties<CompProperties_Network>();
            if (network != null)
            {
                var part = network.networks[Designator_PlaceDrawMouseAttachmentsPatch.GetIndex(tDef)];
                var ioConfig = part.netIOConfig ?? network.generalIOConfig;
                DrawNetIOConfig(ioConfig, center, tDef, placingRot);
            }
        }

        private static void DrawNetIOConfig(NetIOConfig config, IntVec3 center, ThingDef def, Rot4 rot)
        {
            var cells = config.GetCellsFor(rot);
            foreach (var ioCell in cells)
            {
                var cell = center + ioCell.offset;
                var drawPos = cell.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);

                DrawMode(drawPos, ioCell.mode, ioCell.direction);
            }
        }

        private static void DrawMode(Vector3 drawPos, NetworkIOMode ioMode, Rot4 dir)
        {
            switch (ioMode)
            {
                case NetworkIOMode.Input:
                    Graphics.DrawMesh(MeshPool.plane10, drawPos, (dir.AsAngle - 180).ToQuat(), TeleContent.IOArrow, 0);
                    break;
                case NetworkIOMode.Output:
                    Graphics.DrawMesh(MeshPool.plane10, drawPos, dir.AsQuat, TeleContent.IOArrow, 0);
                    break;
                case NetworkIOMode.TwoWay:
                    Graphics.DrawMesh(MeshPool.plane10, drawPos, dir.AsQuat, TeleContent.IOArrowTwoWay, 0);
                    break;
                case NetworkIOMode.Logical:
                    Graphics.DrawMesh(MeshPool.plane10, drawPos, dir.AsQuat, TeleContent.IOArrowLogical, 0);
                    break;
            }
        }
    }


    [HarmonyPatch(typeof(GhostDrawer))]
    [HarmonyPatch("DrawGhostThing")]
    public static class DrawGhostThingPatch
    {
        public static bool Prefix(IntVec3 center, Rot4 rot, ThingDef thingDef, Graphic baseGraphic, Color ghostCol,
            AltitudeLayer drawAltitude)
        {
            if (!thingDef.HasFXExtension(out var extension)) return true;
            if (baseGraphic == null) baseGraphic = thingDef.graphic;
            var graphic = GhostUtility.GhostGraphicFor(baseGraphic, thingDef, ghostCol);
            var loc = GenThing.TrueCenter(center, rot, thingDef.Size, drawAltitude.AltitudeFor());
            TDrawing.Draw(graphic, loc, rot, null, thingDef, null, extension);

            foreach (var t in thingDef.comps)
                t.DrawGhost(center, rot, thingDef, ghostCol, drawAltitude);
            if (thingDef.PlaceWorkers != null)
                foreach (var p in thingDef.PlaceWorkers)
                    p.DrawGhost(thingDef, center, rot, ghostCol);
            return false;
        }
    }

    //
    [HarmonyPatch(typeof(GhostUtility))]
    [HarmonyPatch(nameof(GhostUtility.GhostGraphicFor))]
    public static class GhostUtilityGhostGraphicForPatch
    {
        public static bool Prefix(ref Graphic __result, Graphic baseGraphic, ThingDef thingDef, Color ghostCol, ThingDef stuff = null)
        {
            if (thingDef.HasModExtension<GraphicOverrideExtensions>())
            {
                var extension = thingDef.GetModExtension<GraphicOverrideExtensions>();
                if (extension.ghostGraphic != null)
                {
                    __result = GhostGraphicCopy(extension.ghostGraphic.Graphic, thingDef, ghostCol, stuff);
                    return false;
                }
            }
            //Network Pipe Ghost Graphic Fix
            if (baseGraphic.IsCustomLinked())
            {
                if (thingDef.useSameGraphicForGhost)
                {
                    __result = baseGraphic;
                    return false;
                }

                var seed = 0;
                seed = Gen.HashCombine(seed, baseGraphic);
                seed = Gen.HashCombine(seed, thingDef);
                seed = Gen.HashCombineStruct(seed, ghostCol);
                seed = Gen.HashCombine(seed, stuff);
                if (!GhostUtility.ghostGraphics.TryGetValue(seed, out var value))
                {
                    value = GraphicDatabase.Get<Graphic_Single>(thingDef.uiIconPath, ShaderTypeDefOf.EdgeDetect.Shader,
                        thingDef.graphicData.drawSize, ghostCol);
                    GhostUtility.ghostGraphics.Add(seed, value);
                }

                __result = baseGraphic;
                return false;
            }

            return true;
        }

        private static Graphic GhostGraphicCopy(Graphic? baseGraphic, ThingDef thingDef, Color ghostCol, ThingDef stuff = null)
        {
            var num = 0;
            num = Gen.HashCombine(num, baseGraphic);
            num = Gen.HashCombine(num, thingDef);
            num = Gen.HashCombineStruct(num, ghostCol);
            num = Gen.HashCombine(num, stuff);
            if (!GhostUtility.ghostGraphics.TryGetValue(num, out var graphic))
            {
                if (thingDef.graphicData.Linked || thingDef.IsDoor)
                {
                    graphic = GraphicDatabase.Get<Graphic_Single>(thingDef.uiIconPath, ShaderTypeDefOf.EdgeDetect.Shader, thingDef.graphicData.drawSize, ghostCol);
                }
                else
                {
                    if (baseGraphic == null)
                    {
                        baseGraphic = thingDef.graphic;
                    }
                    GraphicData graphicData = null;
                    if (baseGraphic.data != null)
                    {
                        graphicData = new GraphicData();
                        graphicData.CopyFrom(baseGraphic.data);
                        graphicData.shadowData = null;
                    }
                    string path = baseGraphic.path;
                    Graphic_Appearances graphicAppearances;
                    if ((graphicAppearances = (baseGraphic as Graphic_Appearances)) != null && stuff != null)
                    {
                        graphic = GraphicDatabase.Get<Graphic_Single>(graphicAppearances.SubGraphicFor(stuff).path, ShaderTypeDefOf.EdgeDetect.Shader, thingDef.graphicData.drawSize, ghostCol, Color.white, graphicData, null);
                    }
                    else
                    {
                        graphic = GraphicDatabase.Get(baseGraphic.GetType(), path, ShaderTypeDefOf.EdgeDetect.Shader, baseGraphic.drawSize, ghostCol, Color.white, graphicData, null, null);
                    }
                }
                GhostUtility.ghostGraphics.Add(num, graphic);
            }
            return graphic;
        }
    }


    //
    [HarmonyPatch(typeof(CameraDriver))]
    [HarmonyPatch(nameof(CameraDriver.Update))]
    internal static class CameraDriverPatch
    {
        public static void Postfix(CameraDriver __instance)
        {
            var r = __instance.config.sizeRange;
            var value = Mathf.InverseLerp(r.min, r.max, __instance.rootSize);
            Shader.SetGlobalFloat(TeleShaderIDs.CameraZoom, value);
        }
    }

    [HarmonyPatch(typeof(WindManager))]
    [HarmonyPatch(nameof(WindManager.WindManagerTick))]
    internal static class WindManagerPatch
    {
        public static void Postfix(WindManager __instance)
        {
            Shader.SetGlobalFloat(TeleShaderIDs.WindSpeed, __instance.cachedWindSpeed);
        }
    }
}