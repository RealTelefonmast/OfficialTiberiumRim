namespace TeleCore.Unsorted;

public struct UIPartSizes
{
    public float[] sortedSizes;
    public float totalSize;

    public float this[int ind] => sortedSizes[ind];

    public UIPartSizes(int capacity)
    {
        sortedSizes = new float[capacity];
        totalSize = 0;
    }

    public void Register(int ind, float size)
    {
        sortedSizes[ind] = size;
        totalSize += size;
    }
}