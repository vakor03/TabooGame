using System.Collections.Generic;
using Cards;
using Enums;

namespace Algorithms
{
    public interface IMinimaxAlgorithm
    {
        Card FindBestTurn(List<Card> playersHand, Card[] activeCards, List<Card> possibleCards);
    }
}