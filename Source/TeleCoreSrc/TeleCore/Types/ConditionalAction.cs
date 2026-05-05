using System;

namespace TeleCore.Types;

public class ConditionalAction : IDisposable
{
    private readonly Func<bool> stopCondition;
    private Action action;

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