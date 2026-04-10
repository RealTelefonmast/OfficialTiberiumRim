using System.Runtime.InteropServices;

namespace TeleCore.Lib;

public unsafe class Vertex
{
    public int Degree;
    public int BlockSize;
    public int* Neighbors;
    public int* HashIndex;
    public float* EdgeWeights; // Optional, for weighted graphs
    
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