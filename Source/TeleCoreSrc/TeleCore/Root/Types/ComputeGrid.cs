using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TeleCore.Events;
using TeleCore.Logging;
using TeleCore.Utils;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Rendering;
using Verse;

namespace TeleCore.Types;

public unsafe class ComputeGrid<T> : IExposable, IDisposable where T : unmanaged
{
    private Verse.Map _map;

    private ComputeBuffer _buffer;
    private NativeArray<T> _arr;
    private T* _ptr;

    //
    private int _changesMade;

    public ComputeGrid(Verse.Map map)
    {
        Constructor(map, _ => default);
    }

    public ComputeGrid(Verse.Map map, Func<int, T> factory)
    {
        Constructor(map, factory);
    }

    public bool IsReady { get; private set; }

    public ComputeBuffer DataBuffer => _buffer;

    public NativeArray<T> Grid => _arr;
    public int Length => _arr.Length;

    public T this[int i]
    {
        get => _arr[i];
        private set
        {
            _changesMade++;
            _arr[i] = value;
        }
    }

    public T this[IntVec3 c]
    {
        get => _arr[c.Index(_map)];
        private set
        {
            _changesMade++;
            _arr[c.Index(_map)] = value;
        }
    }

    /// <summary>
    ///     Clear internal <see cref="ComputeBuffer" />
    /// </summary>
    public void Dispose()
    {
        if (!UnityData.IsInMainThread)
        {
            TLog.Warning("ComputeGrid must be disposed on the main thread!");
            return;
        }

        DataBuffer.Dispose();
    }

    public void ExposeData()
    {
    }

    ~ComputeGrid()
    {
        TLog.Warning("Disposing ComputeGrid by GC, make sure to dispose it manually!");
        Dispose();
    }

    public T GetValueByPtr(int idx)
    {
        return _ptr[idx];
    }

    //
    public void ThreadSafeInit()
    {
        if (!UnityData.IsInMainThread)
        {
            TLog.Warning("ComputeGrid must be initialized on the main thread!");
            return;
        }

        IsReady = true;
        _buffer = new ComputeBuffer(Length, Marshal.SizeOf(typeof(T)));
        UpdateBuffer();
    }

    private void Constructor(Verse.Map map, Func<int, T> factory)
    {
        //Clear buffer on quit
        StaticEventHandler.ApplicationQuitEvent += delegate { DataBuffer.Dispose(); };

        //
        this._map = map;

        //
        _arr = new NativeArray<T>(map.cellIndices.NumGridCells, Allocator.Persistent);
        _ptr = (T*)_arr.GetUnsafePtr();

        for (var i = 0; i < _arr.Length; i++) _ptr[i] = factory.Invoke(i);
    }

    public void PullBufferFromGPU()
    {
        if (CheckReadyState(ReadyStateMode.UpdateCPU)) return;
        TLog.Debug("Requesting GPU Data---");
        AsyncGPUReadback.RequestIntoNativeArray(ref _arr, DataBuffer, UpdateInternalCallBack);

        //TFind.TeleRoot.StartCoroutine(UpdateData_Internal());
    }

    public void UpdateBuffer()
    {
        if (CheckReadyState(ReadyStateMode.UpdateBuffer)) return;
        DataBuffer.SetData(_arr);
        _changesMade = 0;
    }

    private IEnumerator UpdateData_Internal()
    {
        yield return null;
        yield return null;
    }

    private void GetPtr()
    {
        AsyncGPUReadback.RequestIntoNativeArray(ref _arr, DataBuffer, UpdateInternalCallBack);
        //(T*) DataBuffer.GetNativeBufferPtr();
        //DataBuffer.GetData(gridArray);
        //gridPtr = (T*) DataBuffer.GetNativeBufferPtr();
    }

    private void UpdateInternalCallBack(AsyncGPUReadbackRequest request)
    {
        TLog.Message($"Received Data From GPU: {!request.hasError}");
    }

    public void SetValues(IEnumerable<IntVec3> positions, Func<IntVec3, T> valueGetter)
    {
        if (CheckReadyState(ReadyStateMode.Setter)) return;

        //
        foreach (var c in positions) _ptr[c.Index(_map)] = valueGetter.Invoke(c);
        DataBuffer.SetData(_arr);
    }

    public void SetValue(IntVec3 c, T t)
    {
        if (CheckReadyState(ReadyStateMode.Setter)) return;

        //
        _ptr[c.Index(_map)] = t;
        DataBuffer.SetData(_arr);
    }

    /// <summary>
    ///     Sets data to the grid array and does not invoke the ComputeBuffer update.
    /// </summary>
    public void SetValues_Array(IEnumerable<IntVec3> positions, Func<IntVec3, T> valueGetter)
    {
        foreach (var c in positions) _ptr[c.Index(_map)] = valueGetter.Invoke(c);
    }

    /// <summary>
    ///     Sets data to the grid array and does not invoke the ComputeBuffer update.
    /// </summary>
    public void SetValue_Array(IntVec3 c, T t)
    {
        _ptr[c.Index(_map)] = t;
    }

    public void ResetValues(IEnumerable<IntVec3> positions, T toVal = default)
    {
        if (CheckReadyState(ReadyStateMode.Setter)) return;

        foreach (var c in positions) _ptr[c.Index(_map)] = toVal;
        DataBuffer.SetData(_arr);
    }

    public void ResetValue(IntVec3 c, T toVal = default)
    {
        if (CheckReadyState(ReadyStateMode.Setter)) return;

        _ptr[c.Index(_map)] = toVal;
        if (!IsReady) return;
        DataBuffer.SetData(_arr);
    }

    private bool CheckReadyState(ReadyStateMode mode)
    {
        if (IsReady) return true;

        var msg = "";
        switch (mode)
        {
            case ReadyStateMode.UpdateCPU:
                msg = "Tried updating the current Grid-Array from GPU data.";
                break;
            case ReadyStateMode.Setter:
                msg = "Tried setting new data into buffer - Try setting values in the grid first and updating later.";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }

        TLog.Warning($"Cannot access Compute-Data without safely initializing first!\n{msg}");
        return false;
    }

    private enum ReadyStateMode
    {
        UpdateCPU,
        UpdateBuffer,
        Setter
    }
}