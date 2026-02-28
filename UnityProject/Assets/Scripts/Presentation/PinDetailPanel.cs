using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Presentation
{
    public class PinDetailPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private RawImage _image;
        [SerializeField] private Button _playAudioButton;
        [SerializeField] private Button _editButton;
        [SerializeField] private Button _closeButton;

        private AudioSource _audioSource;
        private string _currentAudioPath;
        private Action OnEdit;
        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _audioSource = gameObject.AddComponent<AudioSource>();
            _closeButton.onClick.AddListener(() =>
            {
                _audioSource.Stop();
                HideAnimated();
            });
            _playAudioButton.onClick.AddListener(PlayAudio);
            _editButton.onClick.AddListener(() =>
            {
                _audioSource.Stop();
                gameObject.SetActive(false);
                OnEdit?.Invoke();
            });
            gameObject.SetActive(false);
        }

        public void Show(Domain.PinEntity pin, Action onEdit)
        {
            _titleText.text = pin.Title;
            _descriptionText.text = pin.Description;
            _currentAudioPath = pin.AudioPath;
            OnEdit = onEdit;

            _playAudioButton.gameObject.SetActive(!string.IsNullOrEmpty(pin.AudioPath));

            LoadImage(pin.ImagePath);
            _canvasGroup.alpha = 0f;
            transform.localScale = Vector3.one * 0.85f;
            gameObject.SetActive(true);
            _canvasGroup.DOFade(1f, 0.3f).SetEase(Ease.OutCubic);
            transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        }

        public void Hide()
        {
            _audioSource.Stop();
            HideAnimated();
        }

        private void LoadImage(string path)
        {
            if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
            {
                _image.gameObject.SetActive(false);
                return;
            }

            var bytes = System.IO.File.ReadAllBytes(path);
            var tex = new Texture2D(2, 2);
            if (tex.LoadImage(bytes))
            {
                _image.texture = tex;
                _image.gameObject.SetActive(true);

                var rect = _image.GetComponent<RectTransform>();
                float height = rect.sizeDelta.y;
                float width = height * ((float)tex.width / tex.height);
                rect.sizeDelta = new Vector2(width, height);
            }
        }

        private void PlayAudio()
        {
            if (string.IsNullOrEmpty(_currentAudioPath) ||
                !System.IO.File.Exists(_currentAudioPath)) return;

            StartCoroutine(LoadAndPlayAudio(_currentAudioPath));
        }

        private System.Collections.IEnumerator LoadAndPlayAudio(string path)
        {
            var url = "file://" + path;
            var ext = System.IO.Path.GetExtension(path).ToLower();
            var type = AudioType.UNKNOWN;

            if (ext == ".mp3") type = AudioType.MPEG;
            else if (ext == ".wav") type = AudioType.WAV;
            else if (ext == ".ogg") type = AudioType.OGGVORBIS;

            using var req = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip(url, type);
            yield return req.SendWebRequest();
            if (req.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                var clip = UnityEngine.Networking.DownloadHandlerAudioClip
                    .GetContent(req);
                _audioSource.clip = clip;
                _audioSource.Play();
            }
        }
        private void HideAnimated()
        {
            _canvasGroup.DOFade(0f, 0.2f).SetEase(Ease.InCubic);
            transform.DOScale(0.85f, 0.2f)
                .SetEase(Ease.InCubic)
                .OnComplete(() => gameObject.SetActive(false));
        }

        private void OnDestroy()
        {
            transform.DOKill();
            _canvasGroup.DOKill();
        }
    }
}