using System;
using System.IO;

namespace TeleCore.Unsorted;

internal class AnimationFileInfo
{
    private readonly object lockObject = new();
    private string fileName;
    private DateTime lastWriteTime;
    private bool loaded;

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