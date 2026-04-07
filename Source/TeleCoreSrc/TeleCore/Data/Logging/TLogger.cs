using System;
using System.IO;
using TeleCore.Events;
using TeleCore.Utils;
using Verse;

namespace TeleCore.Logging;

[StaticConstructorOnStartup]
public static class TLogger
{
    private static readonly StreamWriter _Writer;

    static TLogger()
    {
        try
        {
            var directory = new DirectoryInfo(GenFilePaths.FolderUnderSaveData("Logging"));
            var file = new FileInfo(Path.Combine(directory.FullName, "TeleCore.txt"));
            if (file.Exists)
            {
                var count = directory.GetFiles().Length;
                var backupPath = $"{Path.GetFileNameWithoutExtension(file.FullName)}_{count - 1}.txt";
                file.CopyTo(backupPath, true);
                file.Delete();
            }

            var _fileStream = file.Open(FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read);
            _Writer = new StreamWriter(_fileStream);
            _Writer.AutoFlush = true;

            StaticEventHandler.ApplicationQuitEvent += delegate
            {
                _Writer.Close();
                _Writer.Dispose();
            };
        }
        catch (Exception ex)
        {
            TLog.Error($"Error while creating logger: {ex}");
        }
    }

    private static string Prefix => $"[{Find.TickManager.TicksGame}]";

    public static void Log(string message)
    {
        try
        {
            _Writer.WriteLine($"{Prefix}: {message}");
        }
        catch (Exception ex)
        {
            TLog.Error($"Error while logging: {ex}");
        }
    }
}