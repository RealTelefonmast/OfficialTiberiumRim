using System;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    /// <summary>
    /// Experimental Updating of custom tiberium related parts
    /// TEST: - possible to sync with RW tick?
    ///       - Hard incompatibility with RW?
    /// 
    /// </summary>
    public class TiberiumRoot : MonoBehaviour
    {
        public virtual void Start()
        {
            try
            {

            }
            catch (Exception arg)
            {
                Log.Error("Error in TiberiumRoot.Start(): " + arg);
            }
        }

        public virtual void Update()
        {
            try
            {

            }
            catch (Exception arg)
            {
                Log.Error("Error in TiberiumRoot.Update(): " + arg);
            }
        }
    }
}
