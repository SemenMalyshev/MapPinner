using Domain;
using UnityEngine;
using DG.Tweening;

namespace Presentation
{
    public class PinView : MonoBehaviour
    {
        private const float BaseOrthographicSize = 4f;
        private const float MaxScale = 2f;
        private const float MinScale = 0.5f;

        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _selectedColor = Color.yellow;

        private int _defaultSortingOrder;
        private float _lastOrthographicSize = 0f;
        private bool _isDragging = false;
        private bool _isSelected;
        private bool _isHovered;

        public PinId Id { get; private set; }
        public int SortingOrder => _spriteRenderer.sortingOrder;

        private void Awake()
        {
            gameObject.AddComponent<PolygonCollider2D>();
        }

        public void Initialize(PinId id, Vector2 position)
        {
            Id = id;
            transform.position = (Vector3)position;
            _defaultSortingOrder = _spriteRenderer.sortingOrder;
            _lastOrthographicSize = -1f;
        }

        public void SetSelected(float orthographicSize)
        {
            _isSelected = true;
            ApplyVisual(orthographicSize, true);
        }

        public void SetNormal(float orthographicSize)
        {
            _isSelected = false;
            ApplyVisual(orthographicSize, true);
        }

        public void SetDetached(float orthographicSize)
        {
            _isDragging = true;
            ApplyVisual(orthographicSize, true);
        }

        public void SetAttached(float orthographicSize)
        {
            _isDragging = false;
            _lastOrthographicSize = 0f;
            ApplyVisual(orthographicSize, true);
        }

        public void SetHovered(float orthographicSize)
        {
            _isHovered = true;
            ApplyVisual(orthographicSize, true);
        }

        public void SetUnhovered(float orthographicSize)
        {
            _isHovered = false;
            ApplyVisual(orthographicSize, true);
        }

        public void UpdateScale(float orthographicSize)
        {
            if (_isDragging) return;
            if (Mathf.Approximately(_lastOrthographicSize, orthographicSize)) return;

            _lastOrthographicSize = orthographicSize;
            ApplyVisual(orthographicSize, false);
        }

        private void ApplyVisual(float orthographicSize, bool animate)
        {
            float scale = Mathf.Clamp(orthographicSize / BaseOrthographicSize, MinScale, MaxScale);
            scale *= _isDragging ? 1.3f : _isSelected || _isHovered ? 1.15f : 1f;
            _spriteRenderer.sortingOrder = _defaultSortingOrder + (_isDragging ? 20 : _isSelected ? 10 : _isHovered ? 5 : 0);
            var color = _isDragging ? new Color(1f, 1f, 1f, 0.7f) : _isSelected || _isHovered ? _selectedColor : _normalColor;

            transform.DOKill();
            _spriteRenderer.DOKill();
            if (animate)
            {
                transform.DOScale(Vector3.one * scale, 0.2f).SetEase(Ease.OutBack);
                _spriteRenderer.DOColor(color, 0.15f);
            }
            else
            {
                transform.localScale = Vector3.one * scale;
                _spriteRenderer.color = color;
            }
        }
        private void OnDestroy()
        {
            transform.DOKill();
            _spriteRenderer.DOKill();
        }
    }
}
