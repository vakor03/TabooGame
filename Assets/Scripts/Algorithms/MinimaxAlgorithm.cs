using System;
using System.Collections.Generic;
using System.Linq;
using Cards;
using Enums;

namespace Algorithms
{
    public class MinimaxAlgorithm : IMinimaxAlgorithm
    {
        public MinimaxAlgorithm(Dictionary<CardRanks, int> cardPoints)
        {
            _cardPoints = cardPoints;
            _algoDepth = 2;
        }


        private int _algoDepth;
        private Dictionary<CardRanks, int> _cardPoints;

        public Card FindBestTurn(List<Card> playersHand, Card[] activeCards, List<Card> possibleCards)
        {
            IGameState initialState = new GameState
            {
                Hand = playersHand.Select(card => new SimpleCard {Rank = card.Rank, Suit = card.Suit}).ToList(),
                Pile = new List<SimpleCard>(),
                ActiveCards = activeCards.Select(card => new SimpleCard {Rank = card.Rank, Suit = card.Suit}).ToList(),
                Deck = possibleCards.Select(card => new SimpleCard {Rank = card.Rank, Suit = card.Suit}).ToList()
            };

            SimpleCard bestTurn = FindBestTurn(initialState);
            if (bestTurn == null)
            {
                return null;
            }
            else
            {
                return playersHand.First(c => c.Rank == bestTurn.Rank && c.Suit == bestTurn.Suit);
            }
        }

        private SimpleCard FindBestTurn(IGameState initialState)
        {
            List<SimpleCard> availableTurns = new List<SimpleCard>();
            foreach (var handCard in initialState.Hand)
            {
                foreach (var activeCard in initialState.ActiveCards)
                {
                    if (activeCard.Rank == handCard.Rank)
                    {
                        availableTurns.Add(handCard);
                    }
                }
            }

            if (initialState.Deck.Count > 0)
            {
                availableTurns.Add(null);
            }

            float maxValue = float.MinValue;
            SimpleCard bestTurn = null;

            foreach (var turn in availableTurns)
            {
                List<IGameState> childStates = initialState.CreateChildAfterTurn(turn, true);
                foreach (var childState in childStates)
                {
                    float currMiniMAxValue = Minimax(childState, _algoDepth, float.MaxValue, float.MinValue, false);
                    if (currMiniMAxValue > maxValue)
                    {
                        maxValue = currMiniMAxValue;
                        bestTurn = turn;
                    }
                }
            }

            return bestTurn;
        }


        private float Minimax(IGameState state, int depth, float alpha, float beta, bool maximizingPlayer)
        {
            if (depth == 0)
            {
                return GetEvaluation(state);
            }

            List<(IGameState state, float chance)> allChildren =
                maximizingPlayer ? GetAllChildren(state) : GetAllOpponentChildren(state);

            if (allChildren.Count == 0)
            {
                return GetEvaluation(state);
            }

            if (maximizingPlayer)
            {
                float maxEvaluation = float.MinValue;
                foreach (var child in allChildren)
                {
                    float evaluation = child.chance * Minimax(child.state, depth - 1, alpha, beta, false);

                    maxEvaluation = Math.Max(maxEvaluation, evaluation);
                    alpha = Math.Max(alpha, evaluation);
                    if (beta <= alpha)
                    {
                        break;
                    }
                }

                return maxEvaluation;
            }
            else
            {
                float minEvaluation = float.MaxValue;
                foreach (var child in allChildren)
                {
                    float evaluation = child.chance * Minimax(child.state, depth - 1, alpha, beta, true);
                    minEvaluation = Math.Min(minEvaluation, evaluation);
                    beta = Math.Min(beta, evaluation);
                    if (beta <= alpha)
                    {
                        break;
                    }
                }

                return minEvaluation;
            }
        }

        private List<(IGameState state, float chance)> GetAllChildren(IGameState initialState)
        {
            List<(IGameState state, float chance)> children = new List<(IGameState state, float chance)>();
            List<SimpleCard> availableTurns = new List<SimpleCard>();
            foreach (var handCard in initialState.Hand)
            {
                foreach (var activeCard in initialState.ActiveCards)
                {
                    if (activeCard.Rank == handCard.Rank)
                    {
                        availableTurns.Add(handCard);
                    }
                }
            }

            if (initialState.Deck.Count > 0)
            {
                availableTurns.Add(null);
            }


            foreach (var turn in availableTurns)
            {
                List<IGameState> childStates = initialState.CreateChildAfterTurn(turn, true);
                foreach (var childState in childStates)
                {
                    children.Add((childState, 1 / (float) availableTurns.Count / childStates.Count));
                }
            }

            return children;
        }

        private List<(IGameState state, float chance)> GetAllOpponentChildren(IGameState initialState)
        {
            List<(IGameState state, float chance)> children = new List<(IGameState state, float chance)>();
            List<(SimpleCard card, float chance)> availableTurns = new();

            foreach (var activeCard in initialState.ActiveCards)
            {
                int cardsInDeck = initialState.Deck.Count(c => c.Rank == activeCard.Rank) - 1;
                if (cardsInDeck > 0)
                {
                    availableTurns.Add((activeCard, cardsInDeck / (float) initialState.Deck.Count));
                }
            }

            foreach (var turn in availableTurns)
            {
                List<IGameState> childStates = initialState.CreateChildAfterTurn(turn.card, false);
                foreach (var childState in childStates)
                {
                    children.Add((childState, turn.chance * 1 / availableTurns.Count / childStates.Count));
                }
            }

            return children;
        }

        private float GetEvaluation(IGameState currentState)
        {
            float evaluation = 0;
            foreach (var pileCards in currentState.Pile)
            {
                if (_cardPoints.ContainsKey(pileCards.Rank))
                {
                    evaluation += _cardPoints[pileCards.Rank];
                }
            }

            foreach (var handCards in currentState.Hand)
            {
                if (_cardPoints.ContainsKey(handCards.Rank))
                {
                    evaluation -= _cardPoints[handCards.Rank];
                }
            }

            return evaluation;
        }
    }
}