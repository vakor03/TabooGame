using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using Players;

namespace PlayerControllers
{
    public class PlayerController : IPlayerController
    {
        public Player this[int i]
        {
            get
            {
                if (i >= _players.Count)
                {
                    throw new IndexOutOfRangeException();
                }

                return _players[i];
            }
        }

        public IReadOnlyList<Player> AllPlayers => _players.AsReadOnly();
        
        private List<Player> _players = new();
        private int _activePlayerId;


        public Player GetActivePlayer()
        {
            if (_players == null || _players.Count == 0)
            {
                throw new NullReferenceException();
            }

            return _players[_activePlayerId];
        }

        public void AddPlayer(Player newPlayer)
        {
            if (newPlayer == null)
            {
                throw new ArgumentNullException();
            }

            _players ??= new List<Player>();
            _players.Add(newPlayer);
        }

        public void RemovePlayer(int playerId)
        {
            if (_players == null || playerId >= _players.Count || playerId < 0)
            {
                throw new ArgumentException();
            }
            
            _players.RemoveAt(playerId);
        }

        public void RemovePlayer(Player player)
        {
            if (_players == null || !_players.Contains(player))
            {
                throw new ArgumentException();
            }

            _players.Remove(player);
        }

        public void RemoveAll()
        {
            _players = new List<Player>();
            _activePlayerId = 0;
        }

        public void SwitchToNext()
        {
            if (_players == null || _players.Count == 0)
            {
                throw new NullReferenceException();
            }
            _activePlayerId = ++_activePlayerId % _players.Count;
        }

        public IEnumerator<Player> GetEnumerator()
        {
            return _players.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}