using System;

namespace TeleCore.Unsorted;

public class ConditionalAction : IDisposable
{
    private Action action;
    private readonly Func<bool> stopCondition;

    public ConditionalAction(Action action, Func<bool> stopCondition)
    {
        this.action = action;
        this.stopCondition = stopCondition;
    }

    public bool ShouldDispose => stopCondition.Invoke();

    public void Dispose()
    {
        action = null;
    }

    public void DoAction()
    {
        action?.Invoke();
    }
}