using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Presentation
{
    public class PinEditPanel : MonoBehaviour
    {
        [Header("Text Input")]
        [SerializeField] private TMP_InputField _titleInput;
        [SerializeField] private TMP_InputField _descriptionInput;

        [Header("Image")]
        [SerializeField] private Button _pickImageButton;
        [SerializeField] private RawImage _imagePreview;

        [Header("Audio")]
        [SerializeField] private Button _pickAudioButton;
        [SerializeField] private TextMeshProUGUI _audioLabel;

        [Header("Buttons")]
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _deleteButton;

        [Header("File System Names")]
        [SerializeField] private string _title;
        [SerializeField] private string _description;

        private string _currentImagePath = "";
        private string _currentAudioPath = "";
        private Action<string, string, string, string> OnSave;
        private Action OnDelete;
        private Action OnClose;
        private RectTransform _rect;
        private CanvasGroup _canvasGroup;
        private float _shownY;
        private float _hiddenY;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _shownY = _rect.anchoredPosition.y;
            _hiddenY = _shownY + 80f;
            _saveButton.onClick.AddListener(OnSaveClicked);
            _closeButton.onClick.AddListener(OnCloseClicked);
            _deleteButton.onClick.AddListener(OnDeleteClicked);
            _pickImageButton.onClick.AddListener(OnPickImage);
            _pickAudioButton.onClick.AddListener(OnPickAudio);
            gameObject.SetActive(false);
        }

        public void Open(
            string title, string description,
            string imagePath, string audioPath,
            Action<string, string, string, string> onSave,
            Action onDelete,
            Action onClose)
        {
            _titleInput.text = title;
            _descriptionInput.text = description;
            _currentImagePath = imagePath;
            _currentAudioPath = audioPath;
            OnSave = onSave;
            OnDelete = onDelete;
            OnClose = onClose;

            _audioLabel.text = string.IsNullOrEmpty(audioPath) ? "No audio choosed" : System.IO.Path.GetFileName(audioPath);

            LoadImagePreview(imagePath);

            _deleteButton.gameObject.SetActive(onDelete != null);

            _rect.anchoredPosition = new Vector2(_rect.anchoredPosition.x, _hiddenY);
            _canvasGroup.alpha = 0f;
            gameObject.SetActive(true);
            _rect.DOAnchorPosY(_shownY, 0.35f).SetEase(Ease.OutCubic);
            _canvasGroup.DOFade(1f, 0.3f).SetEase(Ease.OutCubic);
            _titleInput.Select();
            _titleInput.ActivateInputField();
        }

        private void OnSaveClicked()
        {
            if (string.IsNullOrWhiteSpace(_titleInput.text))
            {
                _titleInput.transform.DOShakePosition(0.4f, new Vector3(8f, 0, 0), 20);
                return;
            }
            OnSave?.Invoke(
                _titleInput.text,
                _descriptionInput.text,
                _currentImagePath,
                _currentAudioPath);
            HideAnimated();
        }

        private void OnCloseClicked()
        {
            OnClose?.Invoke();
            HideAnimated();
        }

        private void OnDeleteClicked()
        {
            OnDelete?.Invoke();
            HideAnimated();
        }

        private void OnPickImage()
        {
            StartCoroutine(PickFileRoutine(
                new[] { "png", "jpg", "jpeg" },
                path =>
                {
                    _currentImagePath = path;
                    LoadImagePreview(path);
                }));
        }

        private void OnPickAudio()
        {
            StartCoroutine(PickFileRoutine(
                new[] { "mp3", "wav", "ogg" },
                path =>
                {
                    _currentAudioPath = path;
                    _audioLabel.text = System.IO.Path.GetFileName(path);
                }));
        }

        private IEnumerator PickFileRoutine(string[] extensions, Action<string> onPicked)
        {
            var extFilter = new SFB.ExtensionFilter[] { new(_title, extensions) };

            var paths = SFB.StandaloneFileBrowser.OpenFilePanel(_description, "", extFilter, false);
            if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
                onPicked(paths[0]);
            yield return null;
        }

        private void LoadImagePreview(string path)
        {
            if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
            {
                _imagePreview.texture = null;
                _imagePreview.gameObject.SetActive(false);
                return;
            }

            var bytes = System.IO.File.ReadAllBytes(path);
            var tex = new Texture2D(2, 2);
            if (tex.LoadImage(bytes))
            {
                _imagePreview.texture = tex;
                _imagePreview.gameObject.SetActive(true);

                var rect = _imagePreview.GetComponent<RectTransform>();
                float height = rect.sizeDelta.y;
                float width = height * ((float)tex.width / tex.height);
                rect.sizeDelta = new Vector2(width, height);
            }
        }
        private void HideAnimated()
        {
            _rect.DOAnchorPosY(_hiddenY, 0.25f).SetEase(Ease.InCubic);
            _canvasGroup.DOFade(0f, 0.25f)
                .SetEase(Ease.InCubic)
                .OnComplete(() => gameObject.SetActive(false));
        }

        private void OnDestroy()
        {
            _rect.DOKill();
            _canvasGroup.DOKill();
        }
    }
}