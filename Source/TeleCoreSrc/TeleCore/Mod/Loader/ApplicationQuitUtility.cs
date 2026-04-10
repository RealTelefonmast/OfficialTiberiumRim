using System;

namespace TeleCore.Loader;

/// <summary>
///     Provides extensions to register custom events whenever the application closes.
/// </summary>
public static class ApplicationQuitUtility
{
    private static Action? _applicationQuitEvent;

    /// <summary>
    ///     Adds a new event to the quit-event chain.
    /// </summary>
    public static void RegisterQuitEvent(Action? action)
    {
        _applicationQuitEvent += action;
    }

    internal static void Notify_ApplicationQuit()
    {
        _applicationQuitEvent?.Invoke();
    }
}