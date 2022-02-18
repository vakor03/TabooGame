using System;
using System.Collections.Generic;
using System.Linq;
using Cards;
using Enums;
using Unity.Mathematics;
using UnityEngine;
using Random = System.Random;

public class Deck : MonoBehaviour
{
    [SerializeField] private Sprite[] cardSprites;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Sprite covering;
    [SerializeField] private Card card;
    public List<Card> Cards => _cards;
    public bool IsEmpty => _cards.Count == 0;
    private List<Card> _cards = new List<Card>();
    private Random _random;
    public Func<bool> TryUpdateDeckFromPile;

    public Collider2D TopCardCollider
    {
        get
        {
            if (Cards.Count == 0)
            {
                return null;
            }

            return Cards[^1].GetComponent<Collider2D>();
        }
    }

    public void Awake()
    {
        _random = new Random();
    }

    public void InitDeck(int decksCount = 1)
    {
        _cards.Clear();
        for (int l = 0; l < decksCount; l++)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 13; j++)
                {
                    Card newCard = Instantiate(card.gameObject, Vector3.zero, quaternion.identity, transform)
                        .GetComponent<Card>();
                    newCard.Rank = (CardRanks) j;
                    newCard.Suit = (CardSuits) i;
                    newCard.Sprite = cardSprites[i * 13 + j];
                    newCard.Covering = covering;

                    _cards.Add(newCard);
                    newCard.DrawCard();
                }
            }

            for (int i = 0; i < 2; i++)
            {
                Card newCard = Instantiate(card.gameObject, Vector3.zero, quaternion.identity, transform)
                    .GetComponent<Card>();
                newCard.Rank = CardRanks.Joker;
                newCard.Suit = (CardSuits) i;
                newCard.Sprite = cardSprites[52 + i];
                newCard.Covering = covering;

                _cards.Add(newCard);
                newCard.DrawCard();
            }
        }


        Draw();
    }

    private void Draw()
    {
        for (var i = 0; i < _cards.Count; i++)
        {
            _cards[i].transform.rotation = Quaternion.identity;
            _cards[i].MoveCard(new Vector3(0, (float) (i * 0.01), (_cards.Count - i) * 0.00001f + 2) +
                               transform.position);
        }
    }

    [ContextMenu("Shuffle")]
    public void Shuffle()
    {
        _cards = _cards.OrderBy(c => _random.Next()).ToList();
        Draw();
    }

    public Card TakeFromTop()
    {
        if (_cards.Count == 1)
        {
            TryUpdateDeckFromPile();
        }

        if (_cards.Count == 0)
        {
            TryUpdateDeckFromPile();
            if (_cards.Count == 0)
            {
                return null;
            }
        }

        Card topCard = _cards[^1];
        _cards.Remove(topCard);
        return topCard;
    }

    public void ShuffleCard(Card card)
    {
        card.transform.SetParent(transform);
        card.IsOpened = false;
        _cards.Add(card);
        Shuffle();
    }
}