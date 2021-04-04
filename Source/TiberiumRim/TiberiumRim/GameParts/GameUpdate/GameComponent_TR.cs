using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class GameComponent_TR : GameComponent
    {
        private static GameObject RootHolder;
        public TiberiumRoot MainRoot;

        public ActionCompositionHolder ActionCompositionHolder;

        public static GameComponent_TR TRComp()
        {
            return Current.Game.GetComponent<GameComponent_TR>();
        }

        public GameComponent_TR(Game game)
        {
            RootHolder = new GameObject("TiberiumRimHolder");
            UnityEngine.Object.DontDestroyOnLoad(RootHolder);
            RootHolder.AddComponent<TiberiumRoot>();
            MainRoot = RootHolder.GetComponent<TiberiumRoot>();

            ActionCompositionHolder = new ActionCompositionHolder();
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
        }

        public override void GameComponentTick()
        {
            ActionCompositionHolder.TickActionComps();
        }

        public override void GameComponentUpdate()
        {
            base.GameComponentUpdate();
        }

        public override void GameComponentOnGUI()
        {
            base.GameComponentOnGUI();
        }
    }
}
