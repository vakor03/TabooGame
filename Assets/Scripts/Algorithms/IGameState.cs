using System;
using System.Collections.Generic;
using Cards;
using Enums;

namespace Algorithms
{
    public interface IGameState : ICloneable
    {
        List<SimpleCard> Pile { get; set; }
        List<SimpleCard> Hand { get; set; }
        List<SimpleCard> ActiveCards { get; set; }
        List<SimpleCard> Deck { get; set; }
        List<IGameState> CreateChildAfterTurn(SimpleCard simpleCardToMove, bool currPlayer);
    }
}