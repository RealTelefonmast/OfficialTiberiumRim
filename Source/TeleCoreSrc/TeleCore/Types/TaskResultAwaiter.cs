using System;
using TeleCore.Types.Interfaces;

namespace TeleCore.Types;

public class TaskResultAwaiter<T> : ITaskAwaiter<ResultState<T>>
{
    private readonly TaskResult<T> _returnType;

    public TaskResultAwaiter(TaskResult<T> returnType)
    {
        _returnType = returnType;
    }

    public bool IsCompleted => _returnType.IsCompleted;

    public ResultState<T> GetResult()
    {
        return _returnType.GetResult();
    }

    public void OnCompleted(Action continuation)
    {
        _returnType.OnCompleted(continuation);
    }

    public void UnsafeOnCompleted(Action continuation)
    {
        OnCompleted(continuation);
    }
}