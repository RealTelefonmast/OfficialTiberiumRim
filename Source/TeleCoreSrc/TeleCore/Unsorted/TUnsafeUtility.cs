using Unity.Collections;

namespace TeleCore.Unsorted;

public static class TUnsafeUtility
{
    public static void CreateOrChangeNativeArr<T>(ref NativeArray<T> arr, T[] values, Allocator allocator)
        where T : struct
    {
        if (values is { Length: > 0 })
        {
            if (arr.IsCreated) arr.Dispose();

            arr = new NativeArray<T>(values, allocator);
        }
    }

    public static void CreateOrChangeNativeArr<T>(ref NativeArray<T> arr, int newLength, Allocator allocator)
        where T : struct
    {
        if (newLength > 0)
        {
            var previous = arr.ToArray();
            if (arr.IsCreated) arr.Dispose();

            arr = new NativeArray<T>(newLength, allocator);
            for (var i = 0; i < previous.Length; i++) arr[i] = previous[i];
        }
    }
}