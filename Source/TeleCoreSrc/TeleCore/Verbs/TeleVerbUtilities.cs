using RimWorld;
using UnityEngine;
using Verse;

namespace TeleCore.Verbs;

public static class TeleVerbUtilities
{
    internal static void DoMuzzleFlash(Verse.Map map, Vector3 origin, LocalTargetInfo intendedTarget, MuzzleFlashProperties flash)
    {
        var vector = origin - intendedTarget.CenterVector3;
        var fleck = FleckMaker.GetDataStatic(origin + flash.offset, map, flash.fleck);
        fleck.scale = flash.scale;
        fleck.rotation = Mathf.Atan2(-vector.z, vector.x) * 57.29578f;
        map.flecks.CreateFleck(fleck);
    }
}