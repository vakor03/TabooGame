using System;
using System.Collections.Generic;
using System.Numerics;
using Cards;
using DG.Tweening;
using GameConfigurations;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace GameMasters
{
    public class GameMaster : IGameMaster
    {
        public Vector3 DeckOffset { get; set; }
        public Deck Deck { get; private set; }
        public List<Card> Pile { get; private set; }
        public Card[] ActiveCards { get; private set; }
        public Action FinishRound { get; set; }
        private Vector3 _pileOffset = new Vector3(0.08f, 0, -0.01f);

        public void Init(Deck defaultDeck, Configuration configuration)
        {
            Deck = defaultDeck;
            Pile = new List<Card>();
            ActiveCards = new Card[configuration.ActiveCardsAmount];
            Deck.InitDeck();
            Deck.transform.position = DeckOffset;
            Deck.TryUpdateDeckFromPile = TryFillDeckFromPile;
        }

        public void UpdateActiveCards(Sequence sequence = null)
        {
            for (int i = ActiveCards.Length - 1; i >= 0; i--)
            {
                if (ActiveCards[i] == null)
                {
                    ActiveCards[i] = Deck.TakeFromTop();
                    if (ActiveCards[i] != null)
                    {
                        ActiveCards[i].transform.SetParent(null);
                        sequence.Append(
                            ActiveCards[i].transform.DOMove(DeckOffset + new Vector3(-(i + 1) * 1.8f, 0, 3), 0.3f));
                    }
                }
            }

            if (sequence == null || !sequence.active)
            {
                OpenActiveCards();
            }
            else
            {
                sequence.onComplete += OpenActiveCards;
            }
        }

        private bool TryFillDeckFromPile()
        {
            if (Pile.Count == 0)
            {
                return false;
            }

            for (int i = Pile.Count - 1; i >= 0; i--)
            {
                Deck.ShuffleCard(Pile[i]);
                Pile.RemoveAt(i);
            }

            return true;
        }

        public void MoveActiveToPile()
        {
            foreach (var card in ActiveCards)
            {
                AddCardToPile(card);
                RemoveActiveCard(card);
            }

            UpdateActiveCards();
        }

        private void UpdatePileUI()
        {
            for (int i = 0; i < Pile.Count; i++)
            {
                Pile[i].transform.DOMove(DeckOffset + new Vector3(3, 0) + i * _pileOffset, 0.3f);
            }
        }

        private void OpenActiveCards()
        {
            foreach (var card in ActiveCards)
            {
                if (card != null)
                {
                    card.IsOpened = true;
                }
            }
        }

        public void RemoveActiveCard(Card card)
        {
            for (int i = 0; i < ActiveCards.Length; i++)
            {
                if (ActiveCards[i] == card)
                {
                    ActiveCards[i] = null;
                    return;
                }
            }

            throw new ArgumentException();
        }

        public void AddCardToPile(Card activeCard)
        {
            Pile.Add(activeCard);
            activeCard.IsOpened = false;
            activeCard.transform.Rotate(new Vector3(0, 0, 1), 90);
            UpdatePileUI();
        }
    }
}