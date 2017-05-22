using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Game.Interfaces;

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
        private const string GameState = "GAME_STATE";

        /// <summary>
        /// Initializes a new instance of Game
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public Game(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override async Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");

            // The StateManager is this actor's private state store.
            // Data stored in the StateManager will be replicated for high-availability for actors that use volatile or persisted state storage.
            // Any serializable object can be saved in the StateManager.
            // For more information, see https://aka.ms/servicefabricactorsstateserialization

            var state = await StateManager.TryGetStateAsync<ActorState>(GameState);

            if (!state.HasValue)
            {
                var newState = new ActorState()
                {
                    Board = new int[9],
                    Winner = "",
                    Players = new List<Tuple<long, string>>(),
                    NextPlayerIndex = 0,
                    NumberOfMoves = 0
                };

                await StateManager.SetStateAsync<ActorState>(GameState, newState);
            }
        }

        public async Task<bool> JoinGameAsync(long playerId, string playerName)
        {
            var state = (await StateManager.TryGetStateAsync<ActorState>(GameState)).Value;

            if (state.Players.Count >= 2 || state.Players.Any(p => p.Item2 == playerName))
            {
                return false;
            }

            state.Players.Add(new Tuple<long, string>(playerId, playerName));

            await StateManager.AddOrUpdateStateAsync<ActorState>(GameState, state, (k, v) => state);

            return true;
        }

        [ReadOnly(true)]
        public async Task<int[]> GetGameBoardAsync()
        {
            var state = await StateManager.TryGetStateAsync<ActorState>(GameState);

            return state.Value.Board;
        }

        [ReadOnly(true)]
        public async Task<string> GetWinnerAsync()
        {
            var state = await StateManager.TryGetStateAsync<ActorState>(GameState);

            return state.Value.Winner;
        }

        public async Task<bool> MakeMoveAsync(long playerId, int x, int y)
        {
            var state = (await StateManager.TryGetStateAsync<ActorState>(GameState)).Value;

            if (x < 0 || x > 2 || y < 0 || y > 2
                || state.Players.Count != 2
                || state.NumberOfMoves >= 9
                || state.Winner != "")
            {
                return false;
            }

            var index = state.Players.FindIndex(p => p.Item1 == playerId);

            if (index != state.NextPlayerIndex)
            {
                return false;
            }

            if (state.Board[y * 3 + x] != 0)
            {
                return false;
            }

            var piece = index * 2 - 1;
            state.Board[y * 3 + x] = piece;
            state.NumberOfMoves++;

            if (HasWon(state, piece * 3))
            {
                state.Winner = state.Players[index].Item2 + " (" + (piece == -1 ? "X" : "O") + ")";
            }
            else if (state.Winner == "" && state.NumberOfMoves >= 9)
            {
                state.Winner = "TIE";
            }

            state.NextPlayerIndex = (state.NextPlayerIndex + 1) % 2;

            await StateManager.AddOrUpdateStateAsync<ActorState>(GameState, state, (k, v) => state);

            return true;
        }

        private static bool HasWon(ActorState state, int sum)
        {
            return state.Board[0] + state.Board[1] + state.Board[2] == sum
                   || state.Board[3] + state.Board[4] + state.Board[5] == sum
                   || state.Board[6] + state.Board[7] + state.Board[8] == sum
                   || state.Board[0] + state.Board[3] + state.Board[6] == sum
                   || state.Board[1] + state.Board[4] + state.Board[7] == sum
                   || state.Board[2] + state.Board[5] + state.Board[8] == sum
                   || state.Board[0] + state.Board[4] + state.Board[8] == sum
                   || state.Board[2] + state.Board[4] + state.Board[6] == sum;
        }
    }
}
