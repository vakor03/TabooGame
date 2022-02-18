using System;
using System.Collections.Generic;
using System.Linq;
using Algorithms;
using Cards;
using DG.Tweening;
using Enums;
using GameConfigurations;
using GameMasters;
using PlayerControllers;
using Players;
using TMPro;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace GameControllers
{
    public class TabooGameController : MonoBehaviour, ITabooGameController
    {
        [SerializeField] private Deck deck;
        [SerializeField] private Camera camera;
        [SerializeField] private TMP_Text label1;
        [SerializeField] private TMP_Text label2;

        private bool _gameIsStarted;

        private IPlayerController _playerController;
        private Configuration _gameConfiguration;
        private IGameMaster _gameMaster;
        private Sequence _mainSequence;

        private Dictionary<CardRanks, int> _cardScores;

        public void Awake()
        {
            _mainSequence = DOTween.Sequence();
            _playerController = new PlayerController();
            _gameConfiguration = new Configuration();
            _gameMaster = new GameMaster();
            _gameMaster.FinishRound += FinishRound;
            _cardScores = new()
            {
                {CardRanks.Ace, 1},
                {CardRanks.King, 12},
                {CardRanks.Queen, 11}
            };

            NewGame();
        }

        public void NewGame()
        {
            _gameMaster.DeckOffset = new Vector3(5, 0);
            _gameMaster.Init(deck, _gameConfiguration);
            label1.text = 0.ToString();
            label2.text = 0.ToString();
            InitPlayers();

            NewRound();
        }

        public void NewRound()
        {
            Sequence sequence = DOTween.Sequence();
            _mainSequence.Restart();
            _gameIsStarted = true;
            _gameMaster.Deck.Shuffle();
            _gameMaster.UpdateActiveCards(sequence);
            DealCards(sequence);
            _playerController[0].StartTurn();
        }

        private void DealCards(Sequence sequence)
        {
            for (int i = 0; i < _gameConfiguration.CardPerPlayerAmount; i++)
            {
                for (int j = 0; j < _gameConfiguration.PlayersCount; j++)
                {
                    _playerController[j].AddCardToHand(_gameMaster.Deck.TakeFromTop(), sequence);
                }
            }
        }

        private void InitPlayers()
        {
            _playerController.AddPlayer(InitRealPlayer(new Vector3(-3, -5.6f, 2)));
            for (int i = 1; i < _gameConfiguration.PlayersCount; i++)
            {
                _playerController.AddPlayer(InitAIPlayer(new Vector3(-3, 5.6f, 2)));
            }
        }

        private Player InitAIPlayer(Vector3 playerOffset)
        {
            IMinimaxAlgorithm minimaxAlgorithm = new MinimaxAlgorithm(_cardScores);
            GameObject realPlayerGameObject = new GameObject();
            AIPlayer aiPlayer = realPlayerGameObject.AddComponent<AIPlayer>();
            aiPlayer._playerOffset = playerOffset;
            aiPlayer.TakeCardFromDeck += () => _gameMaster.Deck.TakeFromTop();
            aiPlayer.MakeTurn = MakeTurn;
            aiPlayer.ActiveCards = () => _gameMaster.ActiveCards;
            aiPlayer.GetAllPossibleCards = () => new List<Card>(_gameMaster.Deck.Cards);
            aiPlayer.MinimaxAlgorithm = minimaxAlgorithm;
            return aiPlayer;
        }
        

        private Player InitRealPlayer(Vector3 playerOffset)
        {
            GameObject realPlayerGameObject = new GameObject();
            RealPlayer realPlayer = realPlayerGameObject.AddComponent<RealPlayer>();
            realPlayer._playerOffset = playerOffset;
            realPlayer.TakeCardFromDeck += () => _gameMaster.Deck.TakeFromTop();
            realPlayer.camera = camera;
            realPlayer.TryMakeTurn = TryMakeTurn;
            realPlayer.TopCardCollider = () => _gameMaster.Deck.TopCardCollider;
            realPlayer.MakeTurn = MakeTurn;
            realPlayer.ActiveCards = () => _gameMaster.ActiveCards;
            return realPlayer;
        }


        private void MakeTurn(Card activeCard, Player activePlayer)
        {
            Sequence sequence = DOTween.Sequence();
            activePlayer.IsActive = false;
            if (activeCard != null)
            {
                activePlayer.RemoveCardFromHand(activeCard);
                if (activeCard.Rank == CardRanks.Jack)
                {
                    _gameMaster.MoveActiveToPile();
                    _gameMaster.AddCardToPile(activeCard);
                }
                else if (activeCard.Rank == CardRanks.Joker)
                {
                    
                    for (int i = 0; i < _gameConfiguration.PlayersCount; i++)
                    {
                        _playerController[i].AddCardToHand(_gameMaster.Deck.TakeFromTop(), sequence);
                    }

                    _gameMaster.MoveActiveToPile();
                    _gameMaster.AddCardToPile(activeCard);
                }
                else
                {
                    activePlayer.AddCardToPile(activeCard);

                    foreach (var card in _gameMaster.ActiveCards)
                    {
                        if (card != null && card.Rank == activeCard.Rank)
                        {
                            _gameMaster.RemoveActiveCard(card);
                            activePlayer.AddCardToPile(card);
                        }
                    }
                }

                _gameMaster.UpdateActiveCards(sequence);
            }

            if (activePlayer.Hand.Count == 0 ||
                !(_playerController[0].AvailableTurnsExists || _playerController[1].AvailableTurnsExists) &&
                _gameMaster.Deck.IsEmpty)
            {
                FinishRound();
            }
            else
            {
                NewTurn();
            }
        }

        private void NewTurn()
        {
            _playerController.SwitchToNext();
            _playerController.GetActivePlayer().StartTurn();
        }

        private bool TryMakeTurn(Card activeCard, Card playerCard)
        {
            if (!_gameMaster.ActiveCards.Contains(activeCard))
            {
                return false;
            }

            if (activeCard.Rank == playerCard.Rank)
            {
                return true;
            }

            return false;
        }

        private void FinishRound()
        {
            Sequence sequence = DOTween.Sequence();
            foreach (var player in _playerController)
            {
                RemoveUnscoredCard(player, sequence);
                CountScores(player);
            }

            //Change later
            label1.text = _playerController[0].Scores.ToString();
            label2.text = _playerController[1].Scores.ToString();

            ClearDeck();
            sequence.onComplete += () => { _gameIsStarted = false; };
        }

        private void CountScores(Player currentPlayer)
        {
            foreach (var card in currentPlayer.Pile)
            {
                currentPlayer.Scores += _cardScores.ContainsKey(card.Rank) ? _cardScores[card.Rank] : 0;
            }

            foreach (var card in currentPlayer.Hand)
            {
                currentPlayer.Scores -= _cardScores.ContainsKey(card.Rank) ? _cardScores[card.Rank] : 0;
            }
        }

        private void RemoveUnscoredCard(Player currentPlayer, Sequence sequence)
        {
            for (var i = currentPlayer.Hand.Count - 1; i >= 0; i--)
            {
                var currentCard = currentPlayer.Hand[i];
                if (!_cardScores.ContainsKey(currentPlayer.Hand[i].Rank))
                {
                    sequence.Append(currentCard.transform.DOMove(_gameMaster.DeckOffset, 0.1f));


                    sequence.onComplete += () =>
                    {
                        _gameMaster.Deck.ShuffleCard(currentCard);
                        currentCard.IsOpened = false;
                        currentPlayer.RemoveCardFromHand(currentCard);
                    };
                }
                else
                {
                    currentCard.IsOpened = true;
                }
            }

            for (var i = currentPlayer.Pile.Count - 1; i >= 0; i--)
            {
                var currentCard = currentPlayer.Pile[i];
                if (!_cardScores.ContainsKey(currentPlayer.Pile[i].Rank))
                {
                    sequence.Append(currentCard.transform.DOMove(_gameMaster.DeckOffset, 0.1f));

                    sequence.onComplete += () =>
                    {
                        _gameMaster.Deck.ShuffleCard(currentCard);
                        currentCard.IsOpened = false;
                        currentPlayer.RemoveCardFromPile(currentCard);
                    };
                }
                else
                {
                    currentCard.IsOpened = true;
                }
            }
        }

        private void Update()
        {
            if (!_gameIsStarted)
            {
                Vector3 cursorPosition = camera.ScreenToWorldPoint(Input.mousePosition);
                //TODO: Restart game on click
                //TODO: Check max point Count
                if (Input.GetMouseButton(0))
                {
                    var collider = Physics2D.OverlapPoint(cursorPosition);
                    Collider2D topCardCollider = _gameMaster.Deck.TopCardCollider;
                    if (collider != null && collider == topCardCollider)
                    {
                        PrepareForNextRound();
                        NewRound();
                    }
                }
            }
        }

        [ContextMenu("Next Round")]
        private void PrepareForNextRound()
        {
            foreach (var player in _playerController)
            {
                ClearAllPlayerCards(player);
            }
        }

        private void ClearAllPlayerCards(Player player)
        {
            for (int i = player.Hand.Count - 1; i >= 0; i--)
            {
                Card card = player.Hand[i];
                player.RemoveCardFromHand(card);
                _gameMaster.Deck.ShuffleCard(card);
            }

            for (int i = player.Pile.Count - 1; i >= 0; i--)
            {
                Card card = player.Pile[i];
                player.RemoveCardFromPile(card);
                _gameMaster.Deck.ShuffleCard(card);
            }
        }

        private void ClearDeck()
        {
            for (int i = 0; i < _gameMaster.ActiveCards.Length; i++)
            {
                Card card = _gameMaster.ActiveCards[i];
                if (card != null)
                {
                    _gameMaster.Deck.ShuffleCard(card);
                    _gameMaster.ActiveCards[i] = null;
                }
            }

            for (int i = 0; i < _gameMaster.Pile.Count; i++)
            {
                Card card = _gameMaster.Pile[i];
                _gameMaster.Deck.ShuffleCard(card);
                _gameMaster.Pile.Remove(card);
            }
        }
    }
}