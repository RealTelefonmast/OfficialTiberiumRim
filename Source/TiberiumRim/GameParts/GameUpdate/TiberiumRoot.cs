using System;
using UnityEngine;
using Verse;

namespace TR
{
    /// <summary>
    /// Experimental Updating of custom tiberium related parts
    /// TEST: - possible to sync with RW tick?
    ///       - Hard incompatibility with RW?
    /// 
    /// </summary>
    public class TiberiumRoot : MonoBehaviour
    {
        private TiberiumTickManager internalTickManager;

        public TiberiumTickManager TickManager => internalTickManager;

        public virtual void Start()
        {
            try
            {
                TRFind.TRoot = this;
                internalTickManager = new TiberiumTickManager();
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
                internalTickManager?.Update();
            }
            catch (Exception arg)
            {
                Log.Error("Error in TiberiumRoot.Update(): " + arg);
            }
        }
    }
}
