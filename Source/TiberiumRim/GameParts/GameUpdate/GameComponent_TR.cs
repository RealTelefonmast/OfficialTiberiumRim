using UnityEngine;
using Verse;

namespace TR
{
    [StaticConstructorOnStartup]
    public class GameComponent_TR : GameComponent
    {
        private static readonly GameObject RootHolder;
        public readonly TiberiumRoot MainRoot;

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
        }

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override void FinalizeInit()
        {
            TRLog.Debug("GameComp TR FinalizeInit");
            base.FinalizeInit();
        }

        public override void GameComponentTick()
        {
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
