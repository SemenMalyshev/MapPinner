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

        public PinId Id { get; private set; }
        public int SortingOrder => _spriteRenderer.sortingOrder;

        private void Awake()
        {
            var col = gameObject.AddComponent<PolygonCollider2D>();
        }

        public void Initialize(PinId id, Vector2 position)
        {
            Id = id;
            transform.position = (Vector3)position;
            //transform.localScale = Vector3.zero;
            //transform.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack);
            _defaultSortingOrder = _spriteRenderer.sortingOrder;
            _lastOrthographicSize = -1f;
        }

        public void SetSelected(float orthographicSize)
        {
            _spriteRenderer.sortingOrder = _defaultSortingOrder + 10;
            float scale = Mathf.Clamp(orthographicSize / BaseOrthographicSize, MinScale, MaxScale);
            transform.DOScale(Vector3.one * scale * 1.15f, 0.2f).SetEase(Ease.OutBack);
        }

        public void SetNormal(float orthographicSize)
        {
            _spriteRenderer.sortingOrder = _defaultSortingOrder;
            float scale = Mathf.Clamp(orthographicSize / BaseOrthographicSize, MinScale, MaxScale);
            transform.DOScale(Vector3.one * scale, 0.2f).SetEase(Ease.OutBack);
        }

        public void SetDetached(float orthographicSize)
        {
            _isDragging = true;
            float scale = Mathf.Clamp(orthographicSize / BaseOrthographicSize, MinScale, MaxScale);
            transform.DOScale(Vector3.one * scale * 1.3f, 0.2f).SetEase(Ease.OutBack);
            _spriteRenderer.DOColor(new Color(1f, 1f, 1f, 0.7f), 0.2f);
        }

        public void SetAttached(float orthographicSize)
        {
            _isDragging = false;
            _lastOrthographicSize = 0f;
            float scale = Mathf.Clamp(orthographicSize / BaseOrthographicSize, MinScale, MaxScale);
            transform.DOScale(Vector3.one * scale, 0.25f).SetEase(Ease.OutBounce);
            _spriteRenderer.DOColor(_normalColor, 0.2f);
        }

        public void SetHovered(float orthographicSize)
        {
            _spriteRenderer.sortingOrder = _defaultSortingOrder + 5;
            _spriteRenderer.DOColor(_selectedColor, 0.15f);
            float scale = Mathf.Clamp(orthographicSize / BaseOrthographicSize, MinScale, MaxScale);
            transform.DOScale(Vector3.one * scale * 1.15f, 0.15f).SetEase(Ease.OutBack);
        }

        public void SetUnhovered(float orthographicSize)
        {
            _spriteRenderer.sortingOrder = _defaultSortingOrder;
            _spriteRenderer.DOColor(_normalColor, 0.15f);
            float scale = Mathf.Clamp(orthographicSize / BaseOrthographicSize, MinScale, MaxScale);
            transform.DOScale(Vector3.one * scale, 0.15f).SetEase(Ease.OutBack);
        }

        public void UpdateScale(float orthographicSize)
        {
            if (_isDragging) return;
            if (Mathf.Approximately(_lastOrthographicSize, orthographicSize)) return;

            _lastOrthographicSize = orthographicSize;
            float scale = Mathf.Clamp(orthographicSize / BaseOrthographicSize, MinScale, MaxScale);
            transform.DOScale(Vector3.one * scale, 0.15f);
        }
        private void OnDestroy()
        {
            transform.DOKill();
            _spriteRenderer.DOKill();
        }
    }
}