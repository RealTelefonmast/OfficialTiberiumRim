namespace TeleCore.Unsorted;

public static unsafe class Array
{
    public static T* New<T>(int size) where T : unmanaged
    {
        var ptr = stackalloc T[size];
        return ptr;
    }
}