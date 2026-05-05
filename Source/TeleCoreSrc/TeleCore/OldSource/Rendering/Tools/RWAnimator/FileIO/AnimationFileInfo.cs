using System;
using System.IO;

namespace TeleCore.Rendering.Tools.RWAnimator.FileIO;

internal class AnimationFileInfo
{
    private string fileName;
    private DateTime lastWriteTime;
    private bool loaded;

    private readonly object lockObject = new();

    public AnimationFileInfo(FileInfo fileInfo)
    {
        FileInfo = fileInfo;
        fileName = fileInfo.Name;
        lastWriteTime = fileInfo.LastWriteTime;
    }

    public FileInfo FileInfo { get; }

    public void LoadData()
    {
        var obj = lockObject;
        lock (obj)
        {
            loaded = true;
        }
    }
}