﻿using System;
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
        private Queue<MainThreadAction> MainThreadActions = new Queue<MainThreadAction>();
        private Action tickActions;

        public TickManager BaseTickManager => Find.TickManager;
        public bool GameRunning => Current.Game != null && !Find.TickManager.Paused;

        public void Update()
        {
            
        }

        public void GameTick()
        {

        }

        public void Tick()
        {
            WorkSubscribedTickActions();
            WorkMainThreadActionQueue();
        }

        public void WorkSubscribedTickActions()
        {
            tickActions?.Invoke();
        }

        public void WorkMainThreadActionQueue()
        {
            if (MainThreadActions.Count <= 0) return;
            var next = MainThreadActions.Dequeue();
            next.DoAction();
        }

        public void Notify_AddNewTickAction(Action action)
        {
            TLog.Debug("Added tick-action!");
            tickActions += action;
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
