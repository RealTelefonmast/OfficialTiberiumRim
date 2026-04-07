using System;
using TeleCore.Loader;
using UnityEngine;

namespace TeleCore;

/// <summary>
///     Experimental Updating of custom core related parts
/// </summary>
public class TeleRoot : MonoBehaviour
{
    public virtual void Start()
    {
        try
        {
         
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