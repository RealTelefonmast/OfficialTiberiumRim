using System;
using System.Runtime.CompilerServices;

namespace TeleCore.Unsorted;

public abstract class AsyncMethodBuilder<TTask, TResult, TResultGet> : IAsyncMethodBuilder<TTask, TResult> where TTask : ITask<TResult, TResultGet>
{
    public abstract TTask Task { get; }
    
    public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
    {
        stateMachine.MoveNext();
    }

    public void SetStateMachine(IAsyncStateMachine stateMachine)
    {
    }

    public void SetResult(TResult result)
    {
        Task.SetResult(result);
    }

    public void SetException(Exception exception)
    {
        Task.SetException(exception);
    }
    
    public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : INotifyCompletion where TStateMachine : IAsyncStateMachine
    {
        awaiter.OnCompleted(stateMachine.MoveNext);
    }

    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine) where TAwaiter : ICriticalNotifyCompletion where TStateMachine : IAsyncStateMachine
    {
        awaiter.UnsafeOnCompleted(stateMachine.MoveNext);
    }
}