using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Cards;
using DG.Tweening;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Players
{
    public class RealPlayer : Player
    {
        public override List<Card> Hand => _hand;
        public Func<Collider2D> TopCardCollider { get; set; }
        public Func<Card, Card, bool> TryMakeTurn { get; set; }

        public Camera camera { get; set; }
        public override List<Card> Pile => _pile;
        public override int Scores { get; set; }
        private Vector3 _cardOffset;
        private Vector3 _pileOffset;

        private List<Card> _hand;
        private List<Card> _pile;

        private Card _activeCard;
        private Vector3 _previousPosition;
        private Collider2D _collider;
        private ContactFilter2D _contactFilter;
        private int _maxWidthInCards;
        private int _takenCardThisRound;

        private void Awake()
        {
            _maxWidthInCards = 6;
            _hand = new List<Card>();
            _pile = new List<Card>();
            _contactFilter = new ContactFilter2D().NoFilter();
            _cardOffset = new(1.6f, 0, -0.01f);
            _pileOffset = _cardOffset * (_maxWidthInCards + 1);
            _takenCardThisRound = 0;
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
            if (card != null)
            {
                card.transform.SetParent(transform);
                _hand.Add(card);
                sequence.Append(
                    Hand[^1].transform.DOLocalMove((Hand.Count - 1) * _cardOffset + _playerOffset, 0.3f));
                card.IsOpened = true;
                if (Hand.Count > 6)
                {
                    UpdateHandUI();
                }
            }
        }

        public override void StartTurn()
        {
            _takenCardThisRound = 0;
            IsActive = true;
        }

        public override void ShowCards()
        {
            UpdateHandUI();
            UpdatePileUI();
        }

        public void Update()
        {
            if (IsActive)
            {
                if (TopCardCollider() == null && !AvailableTurnsExists)
                {
                    MakeTurn(null, this);
                }

                Vector3 cursorPosition = camera.ScreenToWorldPoint(Input.mousePosition);
                cursorPosition.z = 0.001f;
                if (Input.GetMouseButtonDown(0))
                {
                    var collider = Physics2D.OverlapPoint(cursorPosition);
                    Collider2D topCardCollider = TopCardCollider();
                    if (collider != null && collider == topCardCollider)
                    {
                        if (_takenCardThisRound < 1)
                        {
                            Sequence sequence = DOTween.Sequence();
                            AddCardToHand(TakeCardFromDeck(), sequence);
                            _takenCardThisRound++;
                        }
                        else
                        {
                            MakeTurn(null, this);
                        }
                    }
                    else if (collider != null && Hand.Contains(collider.GetComponent<Card>()))
                    {
                        _activeCard = collider.GetComponent<Card>();
                        _previousPosition = _activeCard.transform.position;
                    }
                }

                if (_activeCard != null)
                {
                    _activeCard.transform.position = cursorPosition;
                }

                if (Input.GetMouseButtonUp(0))
                {
                    if (_activeCard is not null)
                    {
                        bool turnAvailable = false;
                        List<Collider2D> colliders2D = new List<Collider2D>();
                        _activeCard.GetComponent<Collider2D>().OverlapCollider(_contactFilter, colliders2D);

                        foreach (var activeCard in colliders2D)
                        {
                            if (ActiveCards().Contains(activeCard.GetComponent<Card>()) &&
                                activeCard.GetComponent<Card>().Rank == _activeCard.Rank)
                            {
                                turnAvailable = true;
                            }
                        }

                        if (turnAvailable)
                        {
                            MakeTurn(_activeCard, this);
                        }
                        else
                        {
                            _activeCard.transform.position = _previousPosition;
                        }

                        _activeCard = null;
                    }
                }
            }
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
                    i * (Hand.Count < _maxWidthInCards ? 1 : (_maxWidthInCards / (float) Hand.Count)) * _cardOffset +
                    _playerOffset, 0.3f);
        }

        private void UpdatePileUI()
        {
            for (int i = 0; i < Pile.Count; i++)
            {
                Pile[i].transform
                    .DOLocalMove(
                        (i) * _cardOffset / 6 * (Pile.Count > 8 ? (8 / (float) Pile.Count) : 1) + _playerOffset +
                        _pileOffset, 0.3f);
            }
        }
    }
}