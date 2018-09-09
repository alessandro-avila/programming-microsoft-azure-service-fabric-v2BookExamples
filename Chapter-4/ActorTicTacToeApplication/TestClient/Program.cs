using Game.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Player.Interfaces;
using System;
using System.Threading.Tasks;

namespace TestClient
{
    public class Program
    {
        static void Main(string[] args)
        {
            var player1 = ActorProxy.Create<IPlayer>(ActorId.CreateRandom(), "fabric:/ActorTicTacToeApplication");
            var player2 = ActorProxy.Create<IPlayer>(ActorId.CreateRandom(), "fabric:/ActorTicTacToeApplication");

            var gameId = ActorId.CreateRandom();
            var gameProxy = ActorProxy.Create<IGame>(gameId, "fabric:/ActorTicTacToeApplication");
            gameProxy.SubscribeAsync(new GameEventHandler());

            var result1 = player1.JoinGameAsync(gameId, "Player 1");
            var result2 = player2.JoinGameAsync(gameId, "Player 2");

            if (result1.Result == false
                || result2.Result == false)
            {
                Console.WriteLine("Failed to join game.");
                return;
            }

            var player1Task = Task.Run(() => { MakeMove(player1, gameProxy, gameId); });
            var player2Task = Task.Run(() => { MakeMove(player2, gameProxy, gameId); });

            var gameTask = Task.Run(
                () =>
                    {
                        string winner = "";
                        while (winner == "")
                        {
                            var board = gameProxy.GetGameBoardAsync().Result;
                            PrintBoard(board);
                            winner = gameProxy.GetWinnerAsync().Result;
                            Task.Delay(1000).Wait();
                        }
                        Console.WriteLine($"Winner is: {winner}");
                    }
                );
            gameTask.Wait();
            Console.Read();
        }

        private static async void MakeMove(IPlayer player, IGame gameProxy, ActorId gameId)
        {
            var rand = new Random();
            while (true)
            {
                await player.MakeMoveAsync(gameId, rand.Next(0, 3), rand.Next(0, 3));
                await Task.Delay(rand.Next(500, 2000));
            }
        }

        private static void PrintBoard(int[] board)
        {
            Console.Clear();
            for (int i = 0; i < board.Length; i++)
            {
                if (board[i] == -1)
                    Console.Write(" X ");
                else if (board[i] == 1)
                    Console.Write(" O ");
                else
                    Console.Write(" . ");
                if ((i + 1) % 3 == 0)
                    Console.WriteLine();
            }
        }
    }
}
