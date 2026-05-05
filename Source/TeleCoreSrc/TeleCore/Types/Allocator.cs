using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TeleCore.Types.Utils;

namespace TeleCore.Types;

public unsafe class DHBAllocator
{
    private const int MAX_BLOCK_SIZE = 1 << 20; // 1MB
    private const int MIN_BLOCK_SIZE = 8;
    private const int SUPERBLOCK_SIZE = 2 * 1024 * 1024; // 2MB

    private readonly FreeBlock*[] freeLists;
    private readonly List<IntPtr> superblocks;

    public DHBAllocator()
    {
        var blockSizeCount = (int)Math.Log(MAX_BLOCK_SIZE / MIN_BLOCK_SIZE, 2) + 1;
        freeLists = new FreeBlock*[blockSizeCount];
        superblocks = new List<IntPtr>();
    }

    public void* Allocate(int size)
    {
        var blockSize = MathT.NextPowerOfTwo(Math.Max(size, MIN_BLOCK_SIZE));
        var index = (int)Math.Log(blockSize / MIN_BLOCK_SIZE, 2);

        if (freeLists[index] == null) AllocateSuperblock(blockSize);

        var block = freeLists[index];
        freeLists[index] = block->Next;
        return block;
    }

    public void Free(void* ptr, int size)
    {
        var blockSize = MathT.NextPowerOfTwo(Math.Max(size, MIN_BLOCK_SIZE));
        var index = (int)Math.Log(blockSize / MIN_BLOCK_SIZE, 2);

        var block = (FreeBlock*)ptr;
        block->Next = freeLists[index];
        freeLists[index] = block;
    }

    private void AllocateSuperblock(int blockSize)
    {
        var superblock = Marshal.AllocHGlobal(SUPERBLOCK_SIZE);
        superblocks.Add(superblock);

        var blockCount = SUPERBLOCK_SIZE / blockSize;
        var index = (int)Math.Log(blockSize / MIN_BLOCK_SIZE, 2);

        for (var i = 0; i < blockCount; i++)
        {
            var block = (FreeBlock*)((byte*)superblock.ToPointer() + i * blockSize);
            block->Next = freeLists[index];
            freeLists[index] = block;
        }
    }

    ~DHBAllocator()
    {
        foreach (var superblock in superblocks) Marshal.FreeHGlobal(superblock);
    }

    private struct FreeBlock
    {
        public FreeBlock* Next;
    }
}