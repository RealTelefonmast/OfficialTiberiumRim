using System;

namespace TeleCore;

/// <summary>
///     Provides extensions to register custom events whenever the application closes.
/// </summary>
public static class ApplicationQuitUtility
{
    internal static Action ApplicationQuitEvent;

    /// <summary>
    ///     Adds a new event to the quit-event chain.
    /// </summary>
    public static void RegisterQuitEvent(Action action)
    {
        ApplicationQuitEvent += action;
    }
}