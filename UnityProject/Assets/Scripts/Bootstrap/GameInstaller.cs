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
        private MapPersistenceService _persistenceService;
        private PinCreationService _pinCreationService;
        private PinViewFactory _pinViewFactory;

        private readonly Dictionary<PinId, PinView> _pinViews = new();
        private PinView _selectedPinView;
        private PinView _draggedPinView;

        private bool IsAnyPanelOpen => _previewPanel.gameObject.activeSelf || _detailPanel.gameObject.activeSelf || _editPanel.gameObject.activeSelf;

        void Start()
        {
            UnityEngine.Application.targetFrameRate = 60;

            var mouseInput = new UnityMouseInput();
            var mapRepository = new JsonMapRepository();
            var selectionService = new PinSelectionService();

            _persistenceService = new MapPersistenceService(mapRepository);
            _pinCreationService = new PinCreationService();
            _cameraNavSystem = new CameraNavigationSystem(mouseInput, _cameraView, _mapView);
            _pinInteractionSystem = new PinInteractionSystem(mouseInput, selectionService, _cameraView);
            _pinViewFactory = new PinViewFactory(_pinPrefab, _pinsContainer);

            _mapState = _persistenceService.Load(new MapId(_mapId));
            foreach (var pin in _mapState.Pins)
                CreatePinView(pin);

            _pinInteractionSystem.OnEmptySpaceClick += HandleEmptySpaceClick;
            _pinInteractionSystem.OnPinSelected += HandlePinSelected;
            _pinInteractionSystem.OnPinDragStart += HandlePinDragStart;
            _pinInteractionSystem.OnPinDragEnd += HandlePinDragEnd;
        }

        void Update()
        {
            if (!IsAnyPanelOpen)
            {
                _cameraNavSystem.Tick();
                foreach (var pinView in _pinViews.Values)
                    pinView.UpdateScale(_cameraView.Camera.orthographicSize);
                _pinInteractionSystem.Tick(_mapState);
            }

            if (_pinInteractionSystem.IsDragging() && _draggedPinView != null)
            {
                var targetPos = (Vector3)_cameraView.ScreenToWorld(UnityEngine.Input.mousePosition);
                _draggedPinView.transform.position = Vector3.Lerp(_draggedPinView.transform.position, targetPos, Time.deltaTime * 10f);
            }
        }

        void OnApplicationQuit()
        {
            if (_mapState != null) _persistenceService.Save(_mapState);
        }



        private void HandleEmptySpaceClick(Vector2 worldPos)
        {
            var newPin = _pinCreationService.CreatePinAt(worldPos);

            _editPanel.Open(title: "", description: "", imagePath: "", audioPath: "", onSave: (title, desc, img, audio) =>
            {
                newPin.Title = title;
                newPin.Description = desc;
                newPin.ImagePath = img;
                newPin.AudioPath = audio;

                _mapState.AddPin(newPin);
                CreatePinView(newPin);
                _persistenceService.Save(_mapState);
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
                    _persistenceService.Save(_mapState);
                },
                onDelete: () => DeletePin(pinEntity),
                () => { }
            );
        }

        private void DeletePin(PinEntity pinEntity)
        {
            if (_pinViews.TryGetValue(pinEntity.Id, out var pinView))
            {
                _pinViewFactory.Destroy(pinView);
                _pinViews.Remove(pinEntity.Id);
            }

            _mapState.RemovePin(pinEntity.Id);
            _persistenceService.Save(_mapState);

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
            _persistenceService.Save(_mapState);
            _draggedPinView = null;
        }

        private PinView CreatePinView(PinEntity entity)
        {
            var view = _pinViewFactory.Create(entity);
            _pinViews.Add(entity.Id, view);
            return view;
        }

        private void SelectPin(PinView newPin)
        {
            if (_selectedPinView != null) _selectedPinView?.SetNormal(_cameraView.Camera.orthographicSize);
            _selectedPinView = newPin;
            if (_selectedPinView != null) _selectedPinView.SetSelected(_cameraView.Camera.orthographicSize);
        }
    }
}