using Game.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Game
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    internal class Game : Actor, IGame
    {
        private readonly string GameStateKey = "GameState";

        /// <summary>
        /// Initializes a new instance of Game
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public Game(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        public Task<bool> AcceptPlayerMoveAsync(long playerId, int x, int y)
        {
            var state = this.StateManager.TryGetStateAsync<GameState>(GameStateKey);
            if (state.Result.HasValue == false)
            {
                return Task.FromResult<bool>(false);
            }

            var gState = state.Result.Value;

            if (x < 0 || x > 2 || y < 0 || y > 2
                || gState.Players.Count != 2
                || gState.NumberOfMoves > 9
                || gState.Winner != "")
            {
                return Task.FromResult<bool>(false);
            }

            int index = gState.Players.FindIndex(p => p.Item1 == playerId);
            if (index != gState.NextPlayerIndex)
            {
                return Task.FromResult<bool>(false);
            }

            if (gState.Board[y * 3 + x] == 0)
            {
                int piece = index * 2 - 1;
                gState.Board[y * 3 + x] = piece;
                gState.NumberOfMoves++;
                if (HasWon(gState.Board, piece * 3))
                {
                    gState.Winner = $"{gState.Players[index].Item2} ( {(piece == -1 ? 'X' : 'O')} )";
                }
                else if (gState.Winner == "" && gState.NumberOfMoves >= 9)
                {
                    gState.Winner = "TIE";
                }
                gState.NextPlayerIndex = (gState.NextPlayerIndex + 1) % 2;

                this.StateManager.SetStateAsync(GameStateKey, gState);

                return Task.FromResult<bool>(true);
            }
            else
            {
                return Task.FromResult<bool>(false);
            }
        }

        private bool HasWon(int[] board, int sum)
        {
            return board[0] + board[1] + board[2] == sum
            || board[3] + board[4] + board[5] == sum
            || board[6] + board[7] + board[8] == sum
            || board[0] + board[3] + board[6] == sum
            || board[1] + board[4] + board[7] == sum
            || board[2] + board[5] + board[8] == sum
            || board[0] + board[4] + board[8] == sum
            || board[2] + board[4] + board[6] == sum;
        }

        public Task<bool> AcceptPlayerToGameAsync(long playerId, string playerName)
        {
            var state = this.StateManager.TryGetStateAsync<GameState>(GameStateKey);
            if (state.Result.HasValue == false)
            {
                return Task.FromResult<bool>(false);
            }

            var gameState = state.Result.Value;
            if (gameState.Players.Count >= 2
                || gameState.Players.FirstOrDefault(p => p.Item2 == playerName) != null)
            {
                return Task.FromResult<bool>(false);
            }
            gameState.Players.Add(new Tuple<long, string>(playerId, playerName));
            this.StateManager.SetStateAsync(GameStateKey, gameState);
            return Task.FromResult<bool>(true);
        }

        [ReadOnly(true)]
        public Task<int[]> GetGameBoardAsync()
        {
            var state = this.StateManager.TryGetStateAsync<GameState>(GameStateKey);
            if (state.Result.HasValue == false)
            {
                return null;
            }
            return Task.FromResult<int[]>(state.Result.Value.Board);
        }

        public Task<string> GetWinnerAsync()
        {
            var state = this.StateManager.TryGetStateAsync<GameState>(GameStateKey);
            if (state.Result.HasValue == false)
            {
                return null;
            }
            return Task.FromResult<string>(state.Result.Value.Winner);
        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, $"Actor [ID: {this.GetActorId()}, Name: {this.GetType().Name}] activating.");

            var state = this.StateManager.TryGetStateAsync<GameState>(GameStateKey);
            if (state.Result.HasValue == false)
            {
                this.StateManager.TryAddStateAsync(GameStateKey, new GameState()
                {
                    Board = new int[9],
                    NextPlayerIndex = 0,
                    NumberOfMoves = 0,
                    Players = new List<Tuple<long, string>>(),
                    Winner = ""
                });
            }

            ActorEventSource.Current.ActorMessage(this, $"Actor [ID: {this.GetActorId()}, Name: {this.GetType().Name}] activated.");

            return Task.FromResult<bool>(true);
        }

    }
}
