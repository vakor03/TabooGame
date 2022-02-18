using System;
using System.Collections.Generic;
using Cards;
using DG.Tweening;
using GameConfigurations;
using UnityEngine;

namespace GameMasters
{
    public interface IGameMaster
    {
        Vector3 DeckOffset { get; set; }
        Deck Deck { get; }
        List<Card> Pile { get; }
        Card[] ActiveCards { get; }
        Action FinishRound { get; set; }
        void Init(Deck defaultDeck, Configuration configuration);
        void UpdateActiveCards(Sequence sequence);
        void MoveActiveToPile();
        void RemoveActiveCard(Card card);
        void AddCardToPile(Card activeCard);
    }
}