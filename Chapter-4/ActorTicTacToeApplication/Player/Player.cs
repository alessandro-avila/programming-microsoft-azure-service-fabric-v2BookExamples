using Game.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Runtime;
using Player.Interfaces;
using System;
using System.Threading.Tasks;

namespace Player
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.None)]
    internal class Player : Actor, IPlayer, IRemindable
    {
        /// <summary>
        /// Initializes a new instance of Player
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public Player(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        public Task<bool> JoinGameAsync(ActorId gameId, string playerName)
        {
            var gameProxy = ActorProxy.Create<IGame>(gameId, "fabric:/ActorTicTacToeApplication");
            return gameProxy.AcceptPlayerToGameAsync(this.Id.GetLongId(), playerName);
        }

        public Task<bool> MakeMoveAsync(ActorId gameId, int x, int y)
        {
            var gameProxy = ActorProxy.Create<IGame>(gameId, "fabric:/ActorTicTacToeApplication");
            return gameProxy.AcceptPlayerMoveAsync(this.Id.GetLongId(), x, y);
        }

        // Timer.
        private IActorTimer mTimer;

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");

            // Timer registration.
            mTimer = this.RegisterTimer(
                Move,                           // Callback method.
                null,                           // Callback state.
                TimeSpan.FromSeconds(5),        // Delay before first callback invocation.
                TimeSpan.FromSeconds(1));       // Callback interval.

            // Reminder registration.
            string task = "Task";
            int amount = 1800;
            Task<IActorReminder> reminder = RegisterReminderAsync(
                task,                           // Reminder name.
                BitConverter.GetBytes(amount),  // Callback state.
                TimeSpan.FromSeconds(3),        // Delay before first callback invocation.
                TimeSpan.FromSeconds(15));      // Callback interval.

            return Task.FromResult<bool>(true);
        }

        protected override Task OnDeactivateAsync()
        {
            // Timer unregistration.
            if (mTimer != null)
            {
                this.UnregisterTimer(mTimer);
            }

            // Reminder unregistration.
            IActorReminder reminder = GetReminder("Task");
            UnregisterReminderAsync(reminder);

            return base.OnDeactivateAsync();
        }

        private Task Move(object arg)
        {
            throw new NotImplementedException();
        }

        public Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
        {
            if (reminderName.Equals("Task"))
            {
                int amount = BitConverter.ToInt32(state, 0);
            }
            return Task.FromResult<bool>(true);
        }
    }
}
