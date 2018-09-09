using Microsoft.ServiceFabric.Actors;

namespace Game.Interfaces
{
    public interface IGameEvents : IActorEvents
    {
        void GameFinished(string winMessage);
    }
}
