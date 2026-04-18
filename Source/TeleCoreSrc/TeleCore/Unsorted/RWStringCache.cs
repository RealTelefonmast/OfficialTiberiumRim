using System.IO;
using Verse;

namespace TeleCore.Unsorted;

public static class RWStringCache
{
    public const string TeleTools = "[TELE TOOLS]";
    public const string DirectoryBrowserTitle = "Directory Selection";
    public const string ToolSelection = "Select a Tool";

    //ClipBoards
    public const string NetworkFilterClipBoard = "NetworkFilterClipBoard";
    public const string NetworkBillClipBoard = "NetworkBillClipBoard";

    public static readonly string DefaultAnimationDefLocation =
        Path.Combine(GenFilePaths.FolderUnderSaveData("Animations"), "Defs");
}
