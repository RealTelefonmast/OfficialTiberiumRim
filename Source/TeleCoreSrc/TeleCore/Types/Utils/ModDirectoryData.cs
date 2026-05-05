using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Verse;

namespace TeleCore.Types.Utils;

internal static class ModDirectoryData
{
    public static DirectoryInfo[] GetAllModDirectoriesInModsFolder()
    {
        return new DirectoryInfo(GenFilePaths.ModsFolderPath).GetDirectories();
    }

    public static IEnumerable<ModContentPack> GetAllModsLoadedAsFolders()
    {
        var folderMods = GetAllModDirectoriesInModsFolder();
        foreach (var mod in LoadedModManager.RunningMods)
            if (folderMods.Any(f => f.Name == mod.FolderName))
                yield return mod;
    }

    public static IEnumerable<ModContentPack> GetAllModsWithTextures()
    {
        foreach (var mod in LoadedModManager.RunningMods)
            if (mod.GetContentHolder<Texture2D>().contentList.Count > 0)
                yield return mod;
    }

    public static IEnumerable<ModContentPack> GetAllModsWithDefs()
    {
        foreach (var mod in LoadedModManager.RunningMods)
            if (mod.defs.Count > 0)
                yield return mod;
    }

    public static List<DirectoryInfo> GetAllDefsOf(ModContentPack mod)
    {
        var list = new List<DirectoryInfo>();
        var modDir = new DirectoryInfo(Path.Combine(GenFilePaths.ModsFolderPath, mod.FolderName));
        var subDirs = modDir.GetDirectories();

        //Test Defs Direct
        var dir = subDirs.FirstOrFallback(d => d.Name == "Defs");
        if (dir != null) list.Add(dir);

        for (var i = 3; i < 8; i++)
        {
            var subDir = subDirs.FirstOrFallback(d => d.Name == $"1.{i}");
        }

        return list;
    }

    public static string PathCapped(DirectoryInfo curDir, DirectoryInfo rootCap)
    {
        var pathText = "";
        var tempDir = new DirectoryInfo(curDir.FullName);
        while (tempDir.Name != rootCap.Parent.Name)
        {
            pathText = $"{tempDir.Name}\\{pathText}";
            tempDir = tempDir.Parent;
        }

        return pathText;
    }
}