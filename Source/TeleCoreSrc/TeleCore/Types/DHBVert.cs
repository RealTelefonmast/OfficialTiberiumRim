using System.Runtime.InteropServices;

namespace TeleCore.Types;

public unsafe class Vertex
{
    public int BlockSize;
    public int Degree;
    public float* EdgeWeights; // Optional, for weighted graphs
    public int* HashIndex;
    public int* Neighbors;

    public Vertex(int initialBlockSize)
    {
        Degree = 0;
        BlockSize = initialBlockSize;
        Neighbors = (int*)Marshal.AllocHGlobal(BlockSize * sizeof(int));
        HashIndex = null; // Will be initialized when needed
        EdgeWeights = null; // Will be initialized if needed
    }

    // Other methods (e.g., for cleanup) would go here
}