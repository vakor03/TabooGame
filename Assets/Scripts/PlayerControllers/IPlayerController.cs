using System.Collections.Generic;
using Players;

namespace PlayerControllers
{
    public interface IPlayerController: IEnumerable<Player>
    {
        Player this[int i] { get; }
        IReadOnlyList<Player> AllPlayers { get; }
        Player GetActivePlayer();
        void AddPlayer(Player newPlayer);
        void RemovePlayer(int playerId);
        void RemovePlayer(Player player);
        void RemoveAll();
        void SwitchToNext();
    }
}