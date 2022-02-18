using System;
using System.Collections.Generic;
using System.Linq;
using Cards;
using DG.Tweening;
using UnityEngine;

namespace Players
{
    public abstract class Player : MonoBehaviour
    {
        public abstract List<Card> Hand { get; }
        public abstract List<Card> Pile { get; }
        public Vector3 _playerOffset { get; set; }
        public abstract int Scores { get; set; }
        public Func<Card> TakeCardFromDeck;
        public Action<Card, Player> MakeTurn;
        public bool IsActive;
        public Func<Card[]> ActiveCards;

        public abstract void AddCardToPile(Card card);
        public abstract void RemoveCardFromPile(Card card);
        public abstract void RemoveCardFromHand(Card card);
        public abstract void AddCardToHand(Card card, Sequence sequence);
        public abstract void StartTurn();
        public abstract void ShowCards();

        public bool AvailableTurnsExists
        {
            get
            {
                Card[] activeCards = ActiveCards();
                foreach (var card in Hand)
                {
                    if (activeCards.Contains(card))
                    {
                        return true;
                    }
                }

                return false;
            }
        }
        
    }
}