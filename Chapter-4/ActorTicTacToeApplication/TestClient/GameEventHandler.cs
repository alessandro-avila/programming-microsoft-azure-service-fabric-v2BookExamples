using Game.Interfaces;
using System;

namespace TestClient
{
    public class GameEventHandler : IGameEvents
    {
        public void GameFinished(string winMessage)
        {
            Console.WriteLine(winMessage);
        }
    }
}
