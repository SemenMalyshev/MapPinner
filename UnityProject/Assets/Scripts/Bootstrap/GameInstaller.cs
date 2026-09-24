using Application;
using Domain;
using Infrastructure;
using Input;
using Presentation;
using System.Collections.Generic;
using UnityEngine;

namespace Bootstrap
{
    public class GameInstaller : MonoBehaviour
    {
        [Header("Objects in Scene")]
        [SerializeField] private MapCameraView _cameraView;
        [SerializeField] private MapView _mapView;
        [SerializeField] private Transform _pinsContainer;

        [Header("UI Panels")]
        [SerializeField] private PinPreviewPanel _previewPanel;
        [SerializeField] private PinDetailPanel _detailPanel;
        [SerializeField] private PinEditPanel _editPanel;

        [Header("Prefabs")]
        [SerializeField] private PinView _pinPrefab;

        [Header("Settings")]
        [SerializeField] private string _mapId = "default";

        private MapState _mapState;
        private CameraNavigationSystem _cameraNavSystem;
        private PinInteractionSystem _pinInteractionSystem;
        private IMapRepository _repository;
        private IMouseInput _input;

        private readonly Dictionary<PinId, PinView> _pinViews = new();
        private PinView _selectedPinView;
        private PinView _draggedPinView;

        private bool IsAnyPanelOpen => _previewPanel.gameObject.activeSelf || _detailPanel.gameObject.activeSelf || _editPanel.gameObject.activeSelf;

        void Start()
        {
            UnityEngine.Application.targetFrameRate = 60;

            _input = new UnityMouseInput();
            var selectionService = new PinSelectionService();

            _repository = new JsonMapRepository();
            _editPanel.SetBrowserReceiver(gameObject);
            _cameraNavSystem = new CameraNavigationSystem(_input, _cameraView, _mapView);
            _pinInteractionSystem = new PinInteractionSystem(_input, selectionService, _cameraView);

            _mapState = new MapState(_repository.Load(new MapId(_mapId)));
            foreach (var pin in _mapState.Pins)
                CreatePinView(pin);

            _pinInteractionSystem.OnEmptySpaceClick += HandleEmptySpaceClick;
            _pinInteractionSystem.OnPinSelected += HandlePinSelected;
            _pinInteractionSystem.OnPinDragStart += HandlePinDragStart;
            _pinInteractionSystem.OnPinDragCancel += HandlePinDragCancel;
            _pinInteractionSystem.OnPinDragEnd += HandlePinDragEnd;
        }

        void Update()
        {
            if (!IsAnyPanelOpen)
            {
                _pinInteractionSystem.Tick(_mapState);
                _cameraNavSystem.Tick(_pinInteractionSystem.IsPressingPin());
                foreach (var pinView in _pinViews.Values)
                    pinView.UpdateScale(_cameraView.Camera.orthographicSize);
            }

            if (_pinInteractionSystem.IsDragging() && _draggedPinView != null)
            {
                var targetPos = (Vector3)_cameraView.ScreenToWorld(_input.GetMousePosition());
                _draggedPinView.transform.position = Vector3.Lerp(_draggedPinView.transform.position, targetPos, Time.deltaTime * 10f);
            }
        }

        private void HandleEmptySpaceClick(Vector2 worldPos)
        {
            var newPin = new PinEntity(PinId.NewId(), worldPos);

            _editPanel.Open(title: "", description: "", imagePath: "", audioPath: "", onSave: (title, desc, img, audio) =>
            {
                newPin.Title = title;
                newPin.Description = desc;
                newPin.ImagePath = img;
                newPin.AudioPath = audio;

                _mapState.AddPin(newPin);
                CreatePinView(newPin);
                SaveMap();
            },
                onDelete: null,
                () => { }
            );
        }

        private void HandlePinSelected(PinEntity pinEntity)
        {
            if (!_pinViews.TryGetValue(pinEntity.Id, out var pinView))
                return;

            SelectPin(pinView);

            _previewPanel.Show(
                pinEntity,
                onReadMore: () => _detailPanel.Show(pinEntity, () => OpenEditForExisting(pinEntity)),
                onEdit: () => OpenEditForExisting(pinEntity)
            );
        }

        private void OpenEditForExisting(PinEntity pinEntity)
        {
            _editPanel.Open(
                title: pinEntity.Title,
                description: pinEntity.Description,
                imagePath: pinEntity.ImagePath,
                audioPath: pinEntity.AudioPath,
                onSave: (title, desc, img, audio) =>
                {
                    pinEntity.Title = title;
                    pinEntity.Description = desc;
                    pinEntity.ImagePath = img;
                    pinEntity.AudioPath = audio;
                    SaveMap();
                },
                onDelete: () => DeletePin(pinEntity),
                () => { }
            );
        }

        private void DeletePin(PinEntity pinEntity)
        {
            if (_pinViews.TryGetValue(pinEntity.Id, out var pinView))
            {
                Destroy(pinView.gameObject);
                _pinViews.Remove(pinEntity.Id);
            }

            _mapState.RemovePin(pinEntity.Id);
            SaveMap();
            BrowserMedia.DeleteImported(pinEntity.ImagePath);
            BrowserMedia.DeleteImported(pinEntity.AudioPath);

            if (_selectedPinView != null &&
                !_pinViews.ContainsValue(_selectedPinView))
                _selectedPinView = null;
        }

        private void HandlePinDragStart(PinEntity pinEntity)
        {
            _previewPanel.Hide();
            _detailPanel.Hide();

            if (_pinViews.TryGetValue(pinEntity.Id, out var pinView))
            {
                _draggedPinView = pinView;
                _draggedPinView.SetDetached(_cameraView.Camera.orthographicSize);
            }
        }

        private void HandlePinDragEnd(PinEntity pinEntity, Vector2 newWorldPos)
        {
            if (_draggedPinView == null) return;

            pinEntity.Position = newWorldPos;
            _draggedPinView.transform.position = new(newWorldPos.x, newWorldPos.y, 0);
            _draggedPinView.SetAttached(_cameraView.Camera.orthographicSize);
            SaveMap();
            _draggedPinView = null;
        }

        private void HandlePinDragCancel(PinEntity pinEntity)
        {
            if (_draggedPinView == null) return;
            _draggedPinView.transform.position = pinEntity.Position;
            _draggedPinView.SetAttached(_cameraView.Camera.orthographicSize);
            _draggedPinView = null;
        }

        private PinView CreatePinView(PinEntity entity)
        {
            var view = Instantiate(_pinPrefab, _pinsContainer);
            view.Initialize(entity.Id, entity.Position);
            _pinViews.Add(entity.Id, view);
            return view;
        }

        private void SaveMap() => _repository.Save(_mapState.ToData());

        public void OnBrowserMediaPicked(string result) => _editPanel.OnBrowserMediaPicked(result);

        private void SelectPin(PinView newPin)
        {
            if (_selectedPinView != null) _selectedPinView?.SetNormal(_cameraView.Camera.orthographicSize);
            _selectedPinView = newPin;
            if (_selectedPinView != null) _selectedPinView.SetSelected(_cameraView.Camera.orthographicSize);
        }
    }
}
