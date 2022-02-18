using DG.Tweening;
using Enums;
using UnityEngine;

namespace Cards
{
    public class Card : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        public bool IsOpened
        {
            get => _isOpened;
            set
            {
                if (value != _isOpened)
                {
                    _isOpened = value;
                    DrawCard();
                }
            }
        }

        public CardRanks Rank { get; set; }
        public CardSuits Suit { get; set; }
        public Sprite Sprite { get; set; }
        public Sprite Covering { get; set; }
        private bool _isOpened;

        public void DrawCard()
        {
            spriteRenderer.sprite = IsOpened ? Sprite : Covering;
        }

        public void MoveCard(Vector3 position)
        {
            transform.DOKill();
            transform.position = position;
        }
    }
}