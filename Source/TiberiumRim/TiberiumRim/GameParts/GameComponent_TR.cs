using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace TiberiumRim
{
    public class GameComponent_TR : GameComponent
    {
        public SampleManager soundManager = new SampleManager();

        public GameComponent_TR(Game game)
        {
        }

        public static GameComponent_TR TRComp()
        {
            return Current.Game.GetComponent<GameComponent_TR>();
        }

        public override void GameComponentUpdate()
        {
            base.GameComponentUpdate();
            soundManager.Update();
        }
    }
}
