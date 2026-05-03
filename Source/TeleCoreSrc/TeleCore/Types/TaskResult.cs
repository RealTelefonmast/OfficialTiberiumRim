using System;

namespace TeleCore.Unsorted;

public struct ResultState<T>
{
    public T Result { get; set; }
    public string State { get; set; }

    public static implicit operator ResultState<T>(T value)
    {
        return new ResultState<T> { Result = value, State = "Default" };
    }
}

//[AsyncMethodBuilder<ITask<TaskResult<T>>()]
public class TaskResult<T> : TaskResult, ITask<T, ResultState<T>>
{
    private Exception? _exception;
    private ResultState<T> _result;

    public bool IsCompleted => _isCompleted;

    public ITaskAwaiter<ResultState<T>> GetAwaiter()
    {
        return new TaskResultAwaiter<T>(this);
    }

    public void SetResult(T result)
    {
        _result = result;
        _result.State = _status;
        _isCompleted = true;
        _continuation?.Invoke();
    }

    public void SetException(Exception exception)
    {
        _exception = exception;
        _isCompleted = true;
        _continuation?.Invoke();
    }

    public void OnCompleted(Action continuation)
    {
        _continuation = continuation;
    }

    public ResultState<T> GetResult()
    {
        if (_exception != null)
            throw _exception;
        return _result;
    }
}

public partial class TaskResult
{
    protected Action? _continuation;
    protected bool _isCompleted;
    protected string _status;

    internal void SetStatus_Int(string status)
    {
        _status = status;
    }
}

public partial class TaskResult
{
    private static TaskResult? _result;

    public static void SetStatus(string status)
    {
        if (_result == null) throw new NullReferenceException("TaskResult is null");
        _result.SetStatus_Int(status);
    }

    internal static void SetCurrent(TaskResult task)
    {
        Console.WriteLine($"Canged current result cache: {task}");
        _result = task;
    }
}