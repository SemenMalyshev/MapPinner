using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Presentation
{
    public class PinPreviewPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _shortDescText;
        [SerializeField] private RawImage _image;
        [SerializeField] private Button _readMoreButton;
        [SerializeField] private Button _editButton;
        [SerializeField] private Button _closeButton;

        private Action OnReadMore;
        private Action OnEdit;
        private RectTransform _rect;
        private Texture2D _imageTexture;
        private float _shownY;
        private float _hiddenY;

        private void Awake()
        {
            _readMoreButton.onClick.AddListener(() =>
            {
                OnReadMore?.Invoke();
                gameObject.SetActive(false);
            });
            _editButton.onClick.AddListener(() =>
            {
                OnEdit?.Invoke();
                gameObject.SetActive(false);
            });
            _closeButton.onClick.AddListener(HideAnimated);
            gameObject.SetActive(false);

            _rect = GetComponent<RectTransform>();
            _shownY = _rect.anchoredPosition.y;
            _hiddenY = _shownY - 300f;
        }

        public void Show(Domain.PinEntity pin, Action onReadMore, Action onEdit)
        {
            OnReadMore = onReadMore;
            OnEdit = onEdit;

            _titleText.text = pin.Title;

            var shortDesc = pin.Description;
            if (shortDesc.Length > 100)
                shortDesc = shortDesc[..100] + "...";
            _shortDescText.text = shortDesc;

            LoadImage(pin.ImagePath);

            _rect.anchoredPosition = new Vector2(_rect.anchoredPosition.x, _hiddenY);
            gameObject.SetActive(true);
            _rect.DOAnchorPosY(_shownY, 0.35f).SetEase(Ease.OutCubic);
        }

        public void Hide() => HideAnimated();

        private void LoadImage(string path)
        {
            if (_imageTexture != null) Destroy(_imageTexture);
            _imageTexture = null;
            if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
            {
                _image.gameObject.SetActive(false);
                return;
            }

            var bytes = System.IO.File.ReadAllBytes(path);
            _imageTexture = new Texture2D(2, 2);
            if (_imageTexture.LoadImage(bytes))
            {
                _image.texture = _imageTexture;
                _image.gameObject.SetActive(true);

                var rect = _image.GetComponent<RectTransform>();
                float height = rect.sizeDelta.y;
                float width = height * ((float)_imageTexture.width / _imageTexture.height);
                rect.sizeDelta = new Vector2(width, height);
            }
            else
            {
                Destroy(_imageTexture);
                _imageTexture = null;
                _image.gameObject.SetActive(false);
            }
        }
        private void HideAnimated()
        {
            _rect.DOAnchorPosY(_hiddenY, 0.25f)
                .SetEase(Ease.InCubic)
                .OnComplete(() => gameObject.SetActive(false));
        }

        private void OnDestroy()
        {
            if (_imageTexture != null) Destroy(_imageTexture);
            _rect.DOKill();
        }
    }
}
