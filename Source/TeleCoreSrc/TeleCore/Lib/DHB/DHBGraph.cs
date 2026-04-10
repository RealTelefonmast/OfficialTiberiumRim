using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.UIElements;

namespace TeleCore.Lib;

//https://drops.dagstuhl.de/storage/00lipics/lipics-vol233-sea2022/LIPIcs.SEA.2022.11/LIPIcs.SEA.2022.11.pdf
public unsafe class DHBGraph
{
    private Dictionary<int, Vertex> vertices;
    private const int HIGH_DEGREE_THRESHOLD = 16 * 64 / sizeof(int); // 16 cache lines
    private const int INITIAL_BLOCK_SIZE = 8;

    public DHBGraph()
    {
        vertices = new Dictionary<int, Vertex>();
    }

    public void AddVertex(int id)
    {
        if (!vertices.ContainsKey(id))
        {
            vertices[id] = new Vertex(INITIAL_BLOCK_SIZE);
        }
    }

    public void AddEdge(int source, int target)
    {
        AddVertex(source);
        Vertex vertex = vertices[source];

        if (vertex.Degree >= vertex.BlockSize)
        {
            ReallocateBlock(vertex);
        }

        if (vertex.Degree >= HIGH_DEGREE_THRESHOLD && vertex.HashIndex == null)
        {
            InitializeHashIndex(vertex);
        }

        if (vertex.HashIndex != null)
        {
            if (LookupNeighbor(vertex, target) != -1)
                return; // Edge already exists

            InsertIntoHashIndex(vertex, target);
        }
        else
        {
            for (int i = 0; i < vertex.Degree; i++)
                if (vertex.Neighbors[i] == target)
                    return; // Edge already exists
        }

        vertex.Neighbors[vertex.Degree++] = target;
    }

    private DHBAllocator allocator = new DHBAllocator();

    private void ReallocateBlock(Vertex vertex)
    {
        int newSize = vertex.BlockSize == 0 ? 1 : vertex.BlockSize * 2;
        int* newNeighbors = (int*)allocator.Allocate(newSize * sizeof(int));
    
        if (vertex.Neighbors != null)
        {
            Buffer.MemoryCopy(vertex.Neighbors, newNeighbors, newSize * sizeof(int), vertex.Degree * sizeof(int));
            allocator.Free(vertex.Neighbors, vertex.BlockSize * sizeof(int));
        }
    
        vertex.Neighbors = newNeighbors;
        vertex.BlockSize = newSize;

        // Similar process for HashIndex if needed
    }

    private void InitializeHashIndex(Vertex vertex)
    {
        vertex.HashIndex = (int*)Marshal.AllocHGlobal(vertex.BlockSize * sizeof(int));
        for (int i = 0; i < vertex.BlockSize; i++)
            vertex.HashIndex[i] = -1;

        for (int i = 0; i < vertex.Degree; i++)
            InsertIntoHashIndex(vertex, vertex.Neighbors[i]);
    }

    private void RebuildHashIndex(Vertex vertex, int newSize)
    {
        int* newHashIndex = (int*)Marshal.AllocHGlobal(newSize * sizeof(int));
        for (int i = 0; i < newSize; i++)
            newHashIndex[i] = -1;

        Marshal.FreeHGlobal((IntPtr)vertex.HashIndex);
        vertex.HashIndex = newHashIndex;

        for (int i = 0; i < vertex.Degree; i++)
            InsertIntoHashIndex(vertex, vertex.Neighbors[i]);
    }

    private int LookupNeighbor(Vertex vertex, int target)
    {
        int hash = Hash(target) & (vertex.BlockSize - 1);
        while (true)
        {
            int index = vertex.HashIndex[hash];
            if (index == -1) return -1; // Not found
            if (index == -2) // Tombstone
            {
                hash = (hash + 1) & (vertex.BlockSize - 1);
                continue;
            }
            if (vertex.Neighbors[index] == target) return index;
            hash = (hash + 1) & (vertex.BlockSize - 1);
        }
    }

    private void InsertIntoHashIndex(Vertex vertex, int target)
    {
        int hash = Hash(target) & (vertex.BlockSize - 1);
        while (vertex.HashIndex[hash] >= 0)
            hash = (hash + 1) & (vertex.BlockSize - 1);
        vertex.HashIndex[hash] = vertex.Degree;
    }

    private int Hash(int value)
    {
        // Simple hash function, can be improved
        return (int)(value * 2654435761 % 2147483647);
    }

    // Other methods like RemoveEdge, GetNeighbors, etc. would go here
}