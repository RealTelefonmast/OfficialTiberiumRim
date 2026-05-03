using System;
using System.Runtime.CompilerServices;

namespace TeleCore.Unsorted;

public class TaskResultMethodBuilder<T> : IAsyncMethodBuilder<TaskResult<T>, T>
{
    public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
    {
        Console.WriteLine("\n Starting ResultMethodBuilder \n");
        stateMachine.MoveNext();
        TaskResult.SetCurrent(Task);
    }

    public void SetStateMachine(IAsyncStateMachine stateMachine)
    {
        Console.WriteLine("\n Setting StateMachine in RMB\n");
    }

    public void SetException(Exception exception)
    {
        Task.SetException(exception);
    }

    public void SetResult(T result)
    {
        Console.WriteLine($"\n Setting Result in RMB: {result} \n");
        Task.SetResult(result);
    }

    public TaskResult<T> Task { get; } = new();

    public void AwaitOnCompleted<TAwaiter, TStateMachine>(
        ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        awaiter.OnCompleted(stateMachine.MoveNext);
        TaskResult.SetCurrent(null);
    }

    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
        ref TAwaiter awaiter, ref TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        awaiter.UnsafeOnCompleted(stateMachine.MoveNext);
        TaskResult.SetCurrent(null);
    }

    public static TaskResultMethodBuilder<T> Create()
    {
        Console.WriteLine("\n Creating ResultMethodBuilder \n");
        return new TaskResultMethodBuilder<T>();
    }
}