using System.IO;
using Verse;

namespace TeleCore.Static;

internal static class StringCache
{
    internal static string TeleTools = "[TELE TOOLS]";
    internal static string DirectoryBrowserTitle = "Directory Selection";
    internal static string ToolSelection = "Select a Tool";

    //ClipBoards
    internal static string NetworkFilterClipBoard = "NetworkFilterClipBoard";
    internal static string NetworkBillClipBoard = "NetworkBillClipBoard";

    //
    internal static string DefaultAnimationDefLocation =
        Path.Combine(GenFilePaths.FolderUnderSaveData("Animations"), "Defs");
}