using System;
using System.Runtime.CompilerServices;

namespace TeleCore.Unsorted;

public interface ITask<TResultSet, TResultGet>
{
    ITaskAwaiter<TResultGet> GetAwaiter();
    void SetResult(TResultSet result);
    void SetException(Exception exception);
    void OnCompleted(Action continuation);
    TResultGet GetResult();
}

public interface ITaskAwaiter<out TResult> : ICriticalNotifyCompletion
{
    bool IsCompleted { get; }
    public TResult GetResult();
}