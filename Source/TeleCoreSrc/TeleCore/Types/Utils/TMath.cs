using UnityEngine;
using Verse;

namespace TeleCore.Types.Utils;

public static class TMath
{
    //Rotate Arrays
    public static T[] FLipHorizontal<T>(this T[] arr, int width, int height)
    {
        var newArr = new T[arr.Length];

        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)
        {
            var curIndex = width * y + x;
            var newX = width - 1 - x;
            var newY = height - 1 - y;
            var newIndex = width * newY + newX;

            newArr[newIndex] = arr[curIndex];
        }

        return newArr;
    }

    public static T[] RotateLeft<T>(this T[] arr, int width, int height)
    {
        var newArr = new T[arr.Length];

        for (var x = width - 1; x >= 0; x--)
        for (var y = height - 1; y >= 0; y--)
        {
            var curIndex = width * y + x;
            var newX = width - 1 - x;
            var transposed = height * newX + y;
            newArr[transposed] = arr[curIndex];
        }

        return newArr;
    }

    public static T[] RotateRight<T>(this T[] arr, int width, int height)
    {
        var newArr = new T[arr.Length];

        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)
        {
            var curIndex = width * y + x;

            //Invert y(as width)
            var newY = height - 1 - y;
            var transposed = height * x + newY;
            newArr[transposed] = arr[curIndex];
        }

        return newArr;
    }

    //Basic
    public static float InverseLerpUnclamped(float a, float b, float value)
    {
        return a == b ? 0 : (value - a) / (b - a);
    }

    public static float NormalizeInRange(float value, float start, float end)
    {
        var width = end - start;
        var offsetValue = value - start;
        return (offsetValue % width + width) % width + start;
    }

    public static float AngleWrapped(this float angle)
    {
        return NormalizeInRange(angle, 0, 360);
    }

    public static float Negate(this float value)
    {
        return value <= 0 ? value : -value;
    }

    //Special
    public static int Mobius(int n)
    {
        if (n == 1) return 1;
        var p = 0;
        for (var i = 2; i <= n; i++)
        {
            if (n % i != 0 || !IsPrime(i)) continue;
            if (n % (i * i) == 0)
                return 0;
            p++;
        }

        return p % 2 != 0 ? -1 : 1;
    }

    //Comparing Math
    public static int Compare(this IntVec3 a, IntVec3 b, int gridWidth)
    {
        var aValue = a.x + a.z * gridWidth;
        var bValue = b.x + b.z * gridWidth;
        return aValue - bValue;
    }

    public static bool IsPrime(int n)
    {
        if (n <= 1) return false;
        if (n == 2) return true;
        if (n % 2 == 0) return false;

        var boundary = (int)global::TeleCore.TMath.Floor(global::TeleCore.TMath.Sqrt(n));
        for (var i = 3; i <= boundary; i += 2)
            if (n % i == 0)
                return false;
        return true;
    }

    //Trigonometry
    public static float OscillateBetween(float minVal, float maxVal, float duration, int currentTick)
    {
        var midVal = (maxVal + minVal) / 2f;
        var amplitude = (maxVal - minVal) / 2f;
        const float phase = Mathf.PI / 2f;

        var value = midVal + amplitude * Mathf.Cos(2f * Mathf.PI * currentTick / duration + phase);

        return value;

        var sineVal = Mathf.Sin((currentTick + duration / 2f) / duration * Mathf.PI) / 2 + 0.5f;
        return Mathf.Lerp(minVal, maxVal, sineVal);
    }

    public static float Cosine(float yMin = 0f, float yMax = 1f, float freq = 1f, float curX = 0f, float period = 2f)
    {
        var mltp = (yMax - yMin) / 2f;
        var height = mltp + yMin;
        var ang = period * Mathf.PI * curX * freq;
        return mltp * Mathf.Cos(ang) + height;
    }

    public static float Cosine2(float yMin = 0f, float yMax = 1f, float xMax = 1f, float xOff = 0f, float curX = 0f)
    {
        var mltp = (yMax - yMin) / 2f;
        var height = mltp + yMin;
        var ang = 1f / (xMax * 2f) * Mathf.PI * (curX - xOff);
        return mltp * Mathf.Cos(ang) + height;
    }

    //Vectors
    public static Vector2 Clamp(this Vector2 vec, Vector2 min, Vector2 max)
    {
        return new Vector2(Mathf.Clamp(vec.x, min.x, max.x), Mathf.Clamp(vec.y, min.x, max.x));
    }

    public static Vector2 Abs(this Vector2 v2)
    {
        return new Vector2(Mathf.Abs(v2.x), Mathf.Abs(v2.y));
    }

    public static float InverseLerp(this Vector3 value, Vector3 a, Vector3 b)
    {
        var AB = b - a;
        var AV = value - a;
        return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
    }

    //Rect
    public static Rect Lerp(this Rect rect1, Rect rect2, float val)
    {
        return new Rect(Vector2.Lerp(rect1.position, rect2.position, val), Vector2.Lerp(rect1.size, rect2.size, val));
    }
}