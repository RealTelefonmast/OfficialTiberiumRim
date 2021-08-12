using System;
using System.Collections.Generic;
using Verse;

namespace TiberiumRim
{
    /// <summary>
    /// Offloaded management of Tiberium-Related content
    /// </summary>
    public class TiberiumUpdateManager
    {
        public OutsourceWorker OutsourceWorker;

        public Queue<MainThreadAction> MainThreadActions = new Queue<MainThreadAction>();

        public TickManager BaseTickManager => Find.TickManager;
        public bool GameRunning => Current.Game != null && !Find.TickManager.Paused;

        public void Update()
        {
            
        }

        public void Tick()
        {
            WorkMainThreadActionQueue();
        }

        public void WorkMainThreadActionQueue()
        {
            if (MainThreadActions.Count <= 0) return;
            var next = MainThreadActions.Dequeue();
            next.DoAction();
        }

        public void Notify_AddNewAction(Action action)
        {
            MainThreadActions.Enqueue(new MainThreadAction(action));
        }
    }

    public class MainThreadAction
    {
        private Action action;

        public MainThreadAction(Action action)
        {
            this.action = action;
        }

        public void DoAction()
        {
            action.Invoke();
        }
    }
}
