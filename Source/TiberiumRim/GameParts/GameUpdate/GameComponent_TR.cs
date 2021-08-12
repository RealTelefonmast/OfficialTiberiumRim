using UnityEngine;
using Verse;

namespace TiberiumRim
{
    [StaticConstructorOnStartup]
    public class GameComponent_TR : GameComponent
    {
        private static GameObject RootHolder;
        public TiberiumRoot MainRoot;

        public ActionCompositionHolder ActionCompositionHolder;
        public TiberiumUpdateManager UpdateManager;

        public static GameComponent_TR TRComp()
        {
            return Current.Game.GetComponent<GameComponent_TR>();
        }

        static GameComponent_TR()
        {
            RootHolder = new GameObject("TiberiumRimHolder");
            UnityEngine.Object.DontDestroyOnLoad(RootHolder);
            RootHolder.AddComponent<TiberiumRoot>();
        }

        public GameComponent_TR(Game game)
        {
            StaticData.Notify_Reload();
            MainRoot = RootHolder.GetComponent<TiberiumRoot>();
            ActionCompositionHolder = new ActionCompositionHolder();
            UpdateManager = new TiberiumUpdateManager();
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override void FinalizeInit()
        {
            Log.Message("GameComp TR FinalizeInit");
            base.FinalizeInit();
        }

        public override void GameComponentTick()
        {
            ActionCompositionHolder.TickActionComps();
            UpdateManager.Tick();
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
