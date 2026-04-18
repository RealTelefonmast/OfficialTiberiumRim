using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml;
using RimWorld;
using TeleCore.Buildings;
using TeleCore.Comps;
using TeleCore.Defs;
using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

public static class GenData
{
    public static bool IsIncendiary(this Verb verb)
    {
        return verb.IsIncendiary_Melee() || verb.IsIncendiary_Ranged();
    }

    public static string Location(this Texture texture)
    {
        if (texture is not Texture2D tx2D)
        {
            TLog.Error($"Tried to find {texture} location as non Texture2D");
            return null;
        }

        return LoadedModManager.RunningMods.SelectMany(m => m.textures.contentList).First(t => t.Value == tx2D).Key;
    }

    public static string Location(this Shader shader)
    {
        return DefDatabase<ShaderTypeDef>.AllDefs.First(t => t.Shader == shader).shaderPath;
    }

    public static IEnumerable<T> AllFlags<T>(this T enumType) where T : Enum
    {
        return enumType.GetAllSelectedItems<T>();
    }

    public static T ObjectValue<T>(this XmlNode node, bool doPostLoad = true)
    {
        return DirectXmlToObject.ObjectFromXml<T>(node, doPostLoad);
    }

    public static bool IsCustomLinked(this Graphic graphic)
    {
        //TODO: Implement custom interface to handle this instead of hardcoding types
        if (graphic is Graphic_LinkedWithSame /*or Graphic_LinkedNetworkStructure*/) return true;
        return false;
    }

    public static bool IsAnonymousType(this Type type, out bool isDisplayClass)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        var hasCompilerGeneratedAttribute = Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false);
        //bool isGeneric = type.IsGenericType;
        var hasCompilerStrings = type.Name.StartsWith("<>") || type.Name.StartsWith("VB$");
        var hasFlags = type.Attributes.HasFlag(TypeAttributes.NotPublic);

        TLog.Debug(
            $"{hasCompilerGeneratedAttribute} && {hasCompilerStrings} && {hasFlags} || {type.Name.Contains("DisplayClass")} | {type.IsGenericType}");
        isDisplayClass = type.Name.Contains("DisplayClass");
        ;
        return hasCompilerGeneratedAttribute && hasCompilerStrings && hasFlags;
    }

    /// <summary>
    ///     Defines whether a structure is powered by electricity and returns whether it actually uses power.
    /// </summary>
    public static bool IsElectricallyPowered(this ThingWithComps thing, out bool usesPower)
    {
        var comp = thing.GetComp<CompPowerTrader>();
        usesPower = comp != null;
        return usesPower && comp.PowerOn;
    }

    /// <summary>
    ///     If the thing uses a PowerComp, returns the PowerOn property, otherwise returns true if no PowerComp exists.
    /// </summary>
    public static bool IsPoweredOn(this ThingWithComps thing)
    {
        return thing.IsElectricallyPowered(out var usesPower) || !usesPower;
    }

    /// <summary>
    ///     Checks whether a thing is reserved by any pawn.
    /// </summary>
    public static bool IsReserved(this Thing thing, Verse.Map onMap, out Pawn reservedBy)
    {
        reservedBy = null;
        if (thing == null) return false;
        var reservations = onMap.reservationManager;
        reservedBy = reservations.ReservationsReadOnly.Find(r => r.Target == thing)?.Claimant;
        return reservedBy != null;
    }

    /// <summary>
    /// </summary>
    public static Room? NeighborRoomOf(this Building building, Room room)
    {
        for (var i = 0; i < 4; i++)
        {
            var cell = GenAdj.CardinalDirections[i] + building.Position;
            var roomAt = cell.GetRoom(building.Map);
            if (roomAt == room)
            {
                var rotOfRoom = new Rot4(i);
                var otherSide = rotOfRoom.Opposite.FacingCell + building.Position;
                var otherRoom = otherSide.GetRoom(building.Map);
                return otherRoom;
            }
        }
        return null;
    }

    /// <summary>
    ///     Returns the current room at a position.
    /// </summary>
    public static Room? GetRoomFast(this IntVec3 pos, Verse.Map map)
    {
        var validRegion = map.regionGrid.GetValidRegionAt_NoRebuild(pos);
        if (validRegion != null && validRegion.type.Passable())
            return validRegion.Room;
        return null;
    }

    /// <summary>
    /// Get the first available room next to the thing.
    /// </summary>
    public static Room? GetRoomIndirect(this Thing thing)
    {
        var room = thing.GetRoom();
        if (room == null)
        {
            var cells = thing.CellsAdjacent8WayAndInside();
            foreach (var c in cells)
            {
                room = c.GetRoom(thing.Map);
                if (room != null)
                {
                    break;
                }
            }
        }
        return room;
    }

    /// <summary>
    /// Get all rooms around the thing.
    /// </summary>
    public static IEnumerable<Room> GetRoomsAround(this Thing thing)
    {
        var cells = thing.CellsAdjacent8WayAndInside();
        foreach (var c in cells)
        {
            var room = c.GetRoom(thing.Map);
            if (room != null)
            {
                yield return room;
                break;
            }
        }
    }

    /// <summary>
    /// Get the desired <see cref="MapInformation" /> based on type of <typeparamref name="T" />.
    /// </summary>
    public static T GetMapInfo<T>(this Verse.Map map) where T : MapInformation
    {
        return map.TeleCore().GetMapInfo<T>();
    }

    /// <summary>
    ///     Get the desired <see cref="Designator" /> based on type of <typeparamref name="T" />.
    /// </summary>
    public static T GetDesignatorFor<T>(BuildableDef def) where T : Designator
    {
        if (StaticData.DESIGNATORS.TryGetValue(def, out var des)) return (T)des;

        des = (Designator)Activator.CreateInstance(typeof(T), def);
        des.icon = def.uiIcon;
        StaticData.DESIGNATORS.Add(def, des);
        return (T)des;
    }

    //Room Tracking
    /// <returns>The main <see cref="Unsorted.RoomTracker" /> object of the <paramref name="room" />.</returns>
    public static RoomTracker RoomTracker(this Room room)
    {
        return room.Map.GetMapInfo<MapInformation_Rooms>()[room];
    }

    /// <summary>
    ///     Get the desired <see cref="RoomComponent" /> based on type of <typeparamref name="T" />.
    /// </summary>
    public static T? GetRoomComp<T>(this Room room) where T : RoomComponent
    {
        return room.RoomTracker()?.GetRoomComp<T>();
    }

    public static RoomComponent GetRoomComp(this Room room, Type type)
    {
        return room.RoomTracker().GetRoomComp(type);
    }

    public static IEnumerable<Thing> OfType<T>(this ListerThings lister) where T : Thing
    {
        return lister.AllThings.Where(t => t is T);
    }

    //
    public static bool IsTeleEntity(this Thing thing)
    {
        return thing is TeleThing or TeleBuilding;
    }

    public static bool IsMetallic(this Thing thing)
    {
        if (thing.def.MadeFromStuff && thing.Stuff.IsMetal) return true;
        return thing.def.IsMetallic();
    }

    public static bool IsMetallic(this ThingDef def)
    {
        if (def.costList.NullOrEmpty()) return false;
        float totalCost = def.costList.Sum(t => t.count);
        float metalCost = def.costList.Find(t => t.thingDef.IsMetal)?.count ?? 0;
        return metalCost / totalCost > 0.5f;
    }

    public static bool IsBuilding(this ThingDef def)
    {
        return def.category == ThingCategory.Building;
    }

    public static bool IsRoomDivider(this ThingDef def)
    {
        return def.passability == Traversability.Impassable && def.Fillage == FillCategory.Full;
    }

    public static bool IsWall(this ThingDef def)
    {
        if (def.category != ThingCategory.Building) return false;
        if (!def.graphicData?.Linked ?? true) return false;
        return (def.graphicData.linkFlags & LinkFlags.Wall) != LinkFlags.None &&
               def.graphicData.linkType == LinkDrawerType.CornerFiller &&
               def is { fillPercent: >= 1f, blockWind: true, coversFloor: true, castEdgeShadows: true, holdsRoof: true, blockLight: true };
    }

    public static bool TryGetComp<T>(this Thing thing, out T comp) where T : ThingComp
    {
        comp = null;
        if (thing is ThingWithComps thingWComps)
        {
            comp = thingWComps.GetComp<T>();
            return comp != null;
        }
        return false;
    }

    public static bool TryGetNetworkPart(this Thing thing, NetworkDef def, out INetworkPart part)
    {
        part = null;
        var networkComp = thing.TryGetComp<CompNetwork>();
        if (networkComp == null) return false;
        part = networkComp[def];
        return part != null;
    }

    //
    public static List<T> ToSingleItemList<T>(this T item)
    {
        return new List<T> { item };
    }

    //
    internal static FXLayerArgs GetArgs(this FXLayer layer)
    {
        return new FXLayerArgs
        {
            categoryTag = layer.data.categoryTag,
            index = layer.Index,
            renderPriority = layer.RenderPriority,
            layerTag = layer.data.layerTag,
            needsPower = layer.data.needsPower,
            data = layer.data,

        };
    }
}