using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Algorithms
{
    public class GameState : IGameState
    {
        public List<SimpleCard> Pile { get; set; }
        public List<SimpleCard> Hand { get; set; }
        public List<SimpleCard> ActiveCards { get; set; }
        public List<SimpleCard> Deck { get; set; }

        public List<IGameState> CreateChildAfterTurn(SimpleCard simpleCardToMove, bool currPlayer)
        {
            List<IGameState> allChildren =
                currPlayer ? FindAllTurns(simpleCardToMove) : FindAllOpponentTurns(simpleCardToMove);


            return allChildren;
        }

        private List<IGameState> FindAllTurns(SimpleCard simpleCardToMove)
        {
            List<IGameState> allChildren = new List<IGameState>();
            if (simpleCardToMove != null)
            {
                IGameState currentChild = (IGameState) Clone();
                SimpleCard[] repeatedCards = ActiveCards.Where(c => c.Rank == simpleCardToMove.Rank).ToArray();
                for (int i = repeatedCards.Length - 1; i >= 0; i--)
                {
                    currentChild.ActiveCards.Remove(repeatedCards[i]);
                    currentChild.Pile.Add(repeatedCards[i]);
                }

                foreach (IEnumerable<SimpleCard> combination in Combinations(Deck, repeatedCards.Length))
                {
                    IGameState newChild = (IGameState) currentChild.Clone();
                    foreach (SimpleCard card in combination)
                    {
                        newChild.ActiveCards.Add(card);
                        newChild.Deck.Remove(card);
                        allChildren.Add(newChild);
                    }
                }
            }
            else
            {
                for (int i = 0; i < Deck.Count; i++)
                {
                    IGameState currentChild = (IGameState) Clone();
                    currentChild.Hand.Add(Deck[i]);
                    currentChild.Deck.RemoveAt(i);
                    allChildren.Add(currentChild);
                }
            }

            return allChildren;
        }

        private List<IGameState> FindAllOpponentTurns(SimpleCard simpleCardToMove)
        {
            List<IGameState> allChildren = new List<IGameState>();
            IGameState currentChild = (IGameState) Clone();
            SimpleCard[] repeatedCards = ActiveCards.Where(c => c.Rank == simpleCardToMove.Rank).ToArray();

            for (int i = repeatedCards.Length - 1; i >= 0; i--)
            {
                currentChild.ActiveCards.Remove(repeatedCards[i]);
            }

            foreach (IEnumerable<SimpleCard> combination in Combinations(Deck, repeatedCards.Length))
            {
                IGameState newChild = (IGameState) currentChild.Clone();
                foreach (SimpleCard card in combination)
                {
                    newChild.ActiveCards.Add(card);
                    newChild.Deck.Remove(card);
                    allChildren.Add(newChild);
                }
            }

            return allChildren;
        }

        public object Clone()
        {
            return new GameState
            {
                Pile = new List<SimpleCard>(this.Pile),
                Hand = new List<SimpleCard>(this.Hand),
                ActiveCards = new List<SimpleCard>(this.ActiveCards),
                Deck = new List<SimpleCard>(this.Deck)
            };
        }


        private static bool NextCombination(IList<int> num, int n, int k)
        {
            bool finished;

            var changed = finished = false;

            if (k <= 0)
            {
                return false;
            }

            for (var i = k - 1; !finished && !changed; i--)
            {
                if (num[i] < n - 1 - (k - 1) + i)
                {
                    num[i]++;

                    if (i < k - 1)
                        for (var j = i + 1; j < k; j++)
                            num[j] = num[j - 1] + 1;
                    changed = true;
                }

                finished = i == 0;
            }

            return changed;
        }

        private static IEnumerable Combinations<T>(IEnumerable<T> elements, int k)
        {
            var elem = elements.ToArray();
            var size = elem.Length;

            if (k > size)
            {
                yield break;
            }

            var numbers = new int[k];

            for (var i = 0; i < k; i++)
            {
                numbers[i] = i;
            }

            do
            {
                yield return numbers.Select(n => elem[n]);
            } while (NextCombination(numbers, size, k));
        }
    }
}