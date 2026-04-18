using System;

namespace TeleCore.Unsorted;

public static class LongTickHandler
{
    /// <summary>
    ///     Enqueues an action to be run once on the main thread when available.
    /// </summary>
    public static void EnqueueActionForMainThread(this Action action)
    {
        TeleUpdateManager.Notify_EnqueueNewSingleAction(action);
    }

    /// <summary>
    ///     Registers an action to be ticked every single tick.
    /// </summary>
    public static void RegisterTickAction(this Action action)
    {
        TeleUpdateManager.Notify_AddNewTickAction(action);
    }

    /// <summary>
    /// </summary>
    public static void AddTaggedAction(this Action action, TeleUpdateManager.TaggedActionType type, string tag)
    {
        TeleUpdateManager.Notify_AddTaggedAction(type, action, tag);
    }
}