using System;
using UnityEngine;
using Verse;

namespace TeleCore.Logging;

public sealed class ModLogger : ILogger
{
    private readonly string _tag;
    private readonly Color _tagColor;
    private readonly Color _debugColor;

    // Wire to a runtime flag after construction: Logger.DebugEnabled = () => MyMod.isDebug;
    public Func<bool> DebugEnabled { get; set; } = static () => false;

    public ModLogger(string tag, Color tagColor, Color debugColor = default)
    {
        _tag        = tag;
        _tagColor   = tagColor;
        _debugColor = debugColor == default ? Color.green : debugColor;
    }

    private string Tag      => _tag.Colorize(_tagColor);
    private string DebugTag => $"{_tag}-Debug".Colorize(_debugColor);

    public void Error(string msg)                   => Log.Error($"{Tag} {msg}");
    public void ErrorOnce(string msg, int id)       => Log.ErrorOnce($"{Tag} {msg}", id);
    public void Warning(string msg)                 => Log.Warning($"{Tag} {msg}");
    public void Message(string msg)                 => Log.Message($"{Tag} {msg}");
    public void Message(string msg, Color color)    => Log.Message($"{_tag.Colorize(color)} {msg}");

    public void Debug(string msg, bool flag = true)
    {
        if (flag && DebugEnabled())
            Log.Message($"{DebugTag} {msg}");
    }

    public void DebugSuccess(string msg) => Log.Message($"{DebugTag} {msg}");

    public void DebugOnce(string msg, int hash)
    {
        var lockObj = Log.logLock;
        lock (lockObj)
        {
            if (!Log.reachedMaxMessagesLimit && Log.usedKeys.Add(hash))
            {
                Debug(msg);
            }
        }
    }
}
