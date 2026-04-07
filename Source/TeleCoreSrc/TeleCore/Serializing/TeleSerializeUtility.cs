using System;

namespace TeleCore.Serializing;

public static class TeleSerializeUtility
{
    //
    public static float[] DeserializeFloat(byte[] data)
    {
        var result = new float[data.Length / 4];
        LoadFloat(data, result.Length, delegate(int i, float dat) { result[i] = dat; });
        return result;
    }

    public static void LoadFloat(byte[] dataArr, int elements, Action<int, float> writer)
    {
        if (dataArr == null || dataArr.Length == 0) return;
        for (var i = 0; i < elements; i++) writer(i, BitConverter.ToSingle(dataArr, i * 4));
    }

    //
    public static byte[] SerializeFloat(int elements, Func<int, float> reader)
    {
        var array = new byte[elements * 4];
        for (var i = 0; i < elements; i++)
        {
            var bytes = BitConverter.GetBytes(reader(i));
            array[i * 4] = bytes[0];
            array[i * 4 + 1] = bytes[1];
            array[i * 4 + 2] = bytes[2];
            array[i * 4 + 3] = bytes[3];
        }

        return array;
    }

    public static byte[] SerializeFloatArray(float[] data)
    {
        return SerializeFloat(data.Length, i => data[i]);
    }

    //
    public static byte[] SerializeUInt(int elements, Func<int, uint> reader)
    {
        var array = new byte[elements * 4];
        for (var i = 0; i < elements; i++)
        {
            var num = reader(i);
            array[i * 4] = (byte) (num & 255);
            array[i * 4 + 1] = (byte) ((num >> 8) & 255);
            array[i * 4 + 2] = (byte) ((num >> 16) & 255);
            array[i * 4 + 3] = (byte) ((num >> 24) & 255);
        }

        return array;
    }

    public static uint[] DeserializeUInt(byte[] data)
    {
        var result = new uint[data.Length / 4];
        LoadUInt(data, result.Length, delegate(int i, uint dat) { result[i] = dat; });
        return result;
    }

    public static void LoadUInt(byte[] arr, int elements, Action<int, uint> writer)
    {
        if (arr == null || arr.Length == 0) return;
        for (var i = 0; i < elements; i++)
            writer(i,
                arr[i * 4] | ((uint) arr[i * 4 + 1] << 8) | ((uint) arr[i * 4 + 2] << 16) |
                ((uint) arr[i * 4 + 3] << 24));
    }
}