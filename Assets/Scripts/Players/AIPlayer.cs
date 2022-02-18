using System;
using System.Collections.Generic;
using Algorithms;
using Cards;
using DG.Tweening;
using UnityEngine;

namespace Players
{
    public class AIPlayer : Player
    {
        public override List<Card> Hand => _hand;
        public override List<Card> Pile => _pile;
        public Func<List<Card>> GetAllPossibleCards;
        public override int Scores { get; set; }
        private Vector3 _cardOffset;
        private Vector3 _pileOffset;

        private List<Card> _hand;
        private List<Card> _pile;

        private int _maxWidthInCards;
        public IMinimaxAlgorithm MinimaxAlgorithm;
        private int _maxCardCouldTake = 1;


        private void Awake()
        {
            _maxWidthInCards = 6;
            _hand = new List<Card>();
            _pile = new List<Card>();
            _cardOffset = new(1.6f, 0, -0.01f);
            _pileOffset = _cardOffset * (_maxWidthInCards + 1);
        }

        public override void AddCardToPile(Card card)
        {
            card.transform.SetParent(transform);
            _pile.Add(card);
            UpdatePileUI();
            card.IsOpened = true;
        }

        public override void RemoveCardFromPile(Card card)
        {
            _pile.Remove(card);
            UpdatePileUI();
        }

        public override void RemoveCardFromHand(Card card)
        {
            _hand.Remove(card);
            UpdateHandUI();
        }

        public override void AddCardToHand(Card card, Sequence sequence)
        {
            if (card == null)
            {
                return;
            }

            card.transform.SetParent(transform);
            _hand.Add(card);
            sequence.Append(Hand[^1].transform.DOLocalMove((Hand.Count - 1) * _cardOffset + _playerOffset, 0.3f));


            if (Hand.Count > 6)
            {
                UpdateHandUI();
            }

            //card.IsOpened = true;
        }

        public override void StartTurn()
        {
            IsActive = true;
            Sequence sequence = DOTween.Sequence();

            for (int i = 0; i < _maxCardCouldTake; i++)
            {
                Card bestTurn = MinimaxAlgorithm.FindBestTurn(Hand, ActiveCards(), GetAllPossibleCards());
                if (bestTurn != null)
                {
                    MakeTurn(bestTurn, this);
                    return;
                }

                AddCardToHand(TakeCardFromDeck(), sequence);
            }


            MakeTurn(MinimaxAlgorithm.FindBestTurn(Hand, ActiveCards(), GetAllPossibleCards()), this);
        }
        private void UpdateHandUI()
        {
            for (int i = 0; i < Hand.Count; i++)
            {
                MoveHandCard(i);
            }
        }

        private void MoveHandCard(int i)
        {
            Hand[i].transform
                .DOLocalMove(
                    i * (Hand.Count < _maxWidthInCards ? 1 : _maxWidthInCards / (float) Hand.Count) * _cardOffset +
                    _playerOffset, 0.3f);
        }

        private void UpdatePileUI()
        {
            for (int i = 0; i < Pile.Count; i++)
            {
                Pile[i].transform
                    .DOLocalMove(
                        i * _cardOffset / 6 * (Pile.Count > 8 ? (8 / (float) Pile.Count) : 1) + _playerOffset +
                        _pileOffset, 0.3f);
            }
        }

        public override void ShowCards()
        {
            foreach (var card in Hand)
            {
                card.IsOpened = true;
            }

            UpdateHandUI();
            UpdatePileUI();
        }
        // private List<Card> GetAllAvailableCards()
        // {
        //     List<Card> availableCards = new List<Card>();
        //     foreach (var handCard in Hand)
        //     {
        //         foreach (var activeCard in ActiveCards())
        //         {
        //             if (activeCard != null && handCard.Rank == activeCard.Rank)
        //             {
        //                 availableCards.Add(handCard);
        //             }
        //         }
        //     }
        //
        //     return availableCards;
        // }
        //
        // private Card ChooseBestCard(List<Card> availableCards)
        // {
        //     return availableCards[0];
        // }
    }
}