using System.Collections.Generic;

namespace TeleCore.Lib;

using System;
using System.Runtime.InteropServices;

public unsafe class DHBAllocator
{
    private const int MAX_BLOCK_SIZE = 1 << 20; // 1MB
    private const int MIN_BLOCK_SIZE = 8;
    private const int SUPERBLOCK_SIZE = 2 * 1024 * 1024; // 2MB

    private struct FreeBlock
    {
        public FreeBlock* Next;
    }

    private FreeBlock*[] freeLists;
    private List<IntPtr> superblocks;

    public DHBAllocator()
    {
        int blockSizeCount = (int)Math.Log(MAX_BLOCK_SIZE / MIN_BLOCK_SIZE, 2) + 1;
        freeLists = new FreeBlock*[blockSizeCount];
        superblocks = new List<IntPtr>();
    }

    public void* Allocate(int size)
    {
        int blockSize = MathT.NextPowerOfTwo(Math.Max(size, MIN_BLOCK_SIZE));
        int index = (int)Math.Log(blockSize / MIN_BLOCK_SIZE, 2);

        if (freeLists[index] == null)
        {
            AllocateSuperblock(blockSize);
        }

        FreeBlock* block = freeLists[index];
        freeLists[index] = block->Next;
        return block;
    }

    public void Free(void* ptr, int size)
    {
        int blockSize = MathT.NextPowerOfTwo(Math.Max(size, MIN_BLOCK_SIZE));
        int index = (int)Math.Log(blockSize / MIN_BLOCK_SIZE, 2);

        FreeBlock* block = (FreeBlock*)ptr;
        block->Next = freeLists[index];
        freeLists[index] = block;
    }

    private void AllocateSuperblock(int blockSize)
    {
        IntPtr superblock = Marshal.AllocHGlobal(SUPERBLOCK_SIZE);
        superblocks.Add(superblock);

        int blockCount = SUPERBLOCK_SIZE / blockSize;
        int index = (int)Math.Log(blockSize / MIN_BLOCK_SIZE, 2);

        for (int i = 0; i < blockCount; i++)
        {
            FreeBlock* block = (FreeBlock*)((byte*)superblock.ToPointer() + i * blockSize);
            block->Next = freeLists[index];
            freeLists[index] = block;
        }
    }
    
    ~DHBAllocator()
    {
        foreach (var superblock in superblocks)
        {
            Marshal.FreeHGlobal(superblock);
        }
    }
}