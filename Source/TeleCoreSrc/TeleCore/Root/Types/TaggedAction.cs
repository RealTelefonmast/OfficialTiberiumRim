using System;

namespace TeleCore.Types;

public class TaggedAction
{
    private readonly Action callback;

    public TaggedAction(Action callback, string tag)
    {
        this.callback = callback;
        Tag = tag;
    }

    public string Tag { get; }

    public void DoAction()
    {
        callback.Invoke();
    }
}