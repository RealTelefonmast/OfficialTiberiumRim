using System;
using UnityEngine;

namespace TeleCore.Unsorted;

/// <summary>
///     Experimental Updating of custom core related parts
/// </summary>
public class TeleRoot : MonoBehaviour
{
    public TeleTickManager TickManager { get; private set; }

    public virtual void Start()
    {
        try
        {
            TickManager = new TeleTickManager();
        }
        catch (Exception arg)
        {
            TLog.Error("Error in TeleRoot.Start(): " + arg);
        }
    }

    public virtual void Update()
    {
        try
        {
            TickManager?.Update();
        }
        catch (Exception arg)
        {
            TLog.Error("Error in TeleRoot.Update(): " + arg);
        }
    }

    private void OnApplicationQuit()
    {
        StaticEventHandler.OnApplicationQuit();
    }
}