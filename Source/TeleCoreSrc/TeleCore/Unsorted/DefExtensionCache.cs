using System.Collections.Generic;
using Verse;

namespace TeleCore.Unsorted;

//TODO: Remove or handle generally
public static class DefExtensionCache
{
    private static readonly Dictionary<Def, (TeleDefExtension, SubMenuExtension, TurretDefExtension, FXDefExtension)> ExtensionsByDef = new();

    //
    public static TeleDefExtension TeleExtension(this Def def)
    {
        return ExtensionsByDef.TryGetValue(def, out var extension) ? extension.Item1 : null;
    }

    public static SubMenuExtension SubMenuExtension(this Def def)
    {
        return ExtensionsByDef.TryGetValue(def, out var extension) ? extension.Item2 : null;
    }

    public static TurretDefExtension TurretExtension(this Def def)
    {
        return ExtensionsByDef.TryGetValue(def, out var extension) ? extension.Item3 : null;
    }

    public static FXDefExtension FXExtension(this Def def)
    {
        return ExtensionsByDef.TryGetValue(def, out var extension) ? extension.Item4 : null;
    }

    //
    public static bool HasTeleExtension(this Def def, out TeleDefExtension extension)
    {
        extension = null;
        if (ExtensionsByDef.TryGetValue(def, out var extensionSet))
        {
            if (extensionSet.Item1 == null) return false;
            extension = extensionSet.Item1;
            return true;
        }

        return false;
    }

    public static bool HasSubMenuExtension(this Def def, out SubMenuExtension extension)
    {
        extension = null;
        if (ExtensionsByDef.TryGetValue(def, out var extensionSet))
        {
            if (extensionSet.Item2 == null) return false;
            extension = extensionSet.Item2;
            return true;
        }

        return false;
    }

    public static bool HasTurretExtension(this Def def, out TurretDefExtension extension)
    {
        extension = null;
        if (ExtensionsByDef.TryGetValue(def, out var extensionSet))
        {
            if (extensionSet.Item3 == null) return false;
            extension = extensionSet.Item3;
            return true;
        }

        return false;
    }

    public static bool HasFXExtension(this Def def, out FXDefExtension extension)
    {
        extension = null;
        if (ExtensionsByDef.TryGetValue(def, out var extensionSet))
        {
            if (extensionSet.Item4 == null) return false;
            extension = extensionSet.Item4;
            return true;
        }

        return false;
    }

    //
    internal static void TryRegister(Def def)
    {
        var teleEx = def.GetModExtension<TeleDefExtension>();
        var menuEx = def.GetModExtension<SubMenuExtension>();
        var turretEx = def.GetModExtension<TurretDefExtension>();
        var fxExtension = def.GetModExtension<FXDefExtension>();
        ExtensionsByDef.Add(def, (teleEx, menuEx, turretEx, fxExtension));
    }
}