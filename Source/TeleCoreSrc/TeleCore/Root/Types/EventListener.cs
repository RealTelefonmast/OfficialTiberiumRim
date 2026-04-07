using System;
using UnityEngine;

namespace TeleCore.Types;

public class EventListener
{
    private readonly Action callback;
    private readonly Func<Event, bool> listener;

    public EventListener(Func<Event, bool> listener, Action callBack, string ID)
    {
        this.listener = listener;
        callback = callBack;
        this.ID = ID;
    }

    public string ID { get; }

    public void ListenToEvent(Event curEvent)
    {
        if (listener.Invoke(curEvent)) callback.Invoke();
    }
}