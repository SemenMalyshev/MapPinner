using Domain;
using Input;
using Presentation;
using UnityEngine;

namespace Application
{
    public class PinInteractionSystem
    {
        private const float ClickMaxDistance = 5f;
        private const float HoldThreshold = 0.3f;
        private const float HitTolerance = 20f;

        private enum State { Idle, Pressed, Dragging }
        private State _currentState = State.Idle;

        private readonly IMouseInput _input;
        private readonly PinSelectionService _selectionService;
        private readonly MapCameraView _cameraView;

        private Vector2 _pressStartPosition;
        private float _pressTime;
        private PinEntity _pressedPin;
        private PinView _hoveredPinView;

        public System.Action<PinEntity> OnPinSelected;
        public System.Action<PinEntity> OnPinDragStart;
        public System.Action<PinEntity, Vector2> OnPinDragEnd;
        public System.Action<Vector2> OnEmptySpaceClick;

        public PinInteractionSystem(IMouseInput input, PinSelectionService selectionService, MapCameraView cameraView)
        {
            _input = input;
            _selectionService = selectionService;
            _cameraView = cameraView;
        }

        public void Tick(MapState mapState)
        {
            var mousePos = _input.GetMousePosition();
            var pinUnderMouse = _selectionService.FindPinAtScreenPosition(
                new Vector2(mousePos.x, mousePos.y),
                mapState.Pins,
                wp =>
                {
                    var sp = _cameraView.WorldToScreen(wp);
                    return new Vector2(sp.x, sp.y);
                },
                HitTolerance
            );

            switch (_currentState)
            {
                case State.Idle:
                    if (_input.GetLeftMouseButtonDown())
                    {
                        _currentState = State.Pressed;
                        _pressTime = 0;
                        _pressedPin = pinUnderMouse;
                        _pressStartPosition = _input.GetMousePosition();
                    }
                    break;

                case State.Pressed:
                    _pressTime += Time.deltaTime;

                    if (!_input.IsLeftMouseButtonPressed())
                    {
                        float moved = Vector2.Distance(_pressStartPosition, _input.GetMousePosition());
                        if (moved < ClickMaxDistance)
                        {
                            if (_pressedPin != null)
                                OnPinSelected?.Invoke(_pressedPin);
                            else
                                OnEmptySpaceClick?.Invoke(_cameraView.ScreenToWorld(mousePos));
                        }
                        _currentState = State.Idle;
                    }
                    else if (_pressTime > HoldThreshold && _pressedPin != null)
                    {
                        _currentState = State.Dragging;
                        OnPinDragStart?.Invoke(_pressedPin);
                    }
                    break;

                case State.Dragging:
                    if (_input.GetLeftMouseButtonUp())
                    {
                        var worldPos = _cameraView.ScreenToWorld(new Vector2(mousePos.x, mousePos.y));
                        OnPinDragEnd?.Invoke(_pressedPin, new Vector2(worldPos.x, worldPos.y));
                        _currentState = State.Idle;
                    }
                    break;
            }

            UpdateHoveredPins();

        }

        private void UpdateHoveredPins()
        {

            if (IsDragging()) return;

            var mousePos = _input.GetMousePosition();
            var worldPoint = _cameraView.Camera.ScreenToWorldPoint(mousePos);

            var hits = Physics2D.OverlapPointAll(worldPoint);

            PinView bestPin = null;
            int bestOrder = int.MinValue;

            foreach (var hit in hits)
            {
                var pinView = hit.GetComponent<PinView>();
                if (pinView == null) continue;

                if (pinView.SortingOrder > bestOrder)
                {
                    bestOrder = pinView.SortingOrder;
                    bestPin = pinView;
                }
            }

            if (bestPin != _hoveredPinView)
            {
                if (_hoveredPinView != null) _hoveredPinView.SetUnhovered(_cameraView.Camera.orthographicSize);
                _hoveredPinView = bestPin;
                if (_hoveredPinView != null) _hoveredPinView.SetHovered(_cameraView.Camera.orthographicSize);
            }
        }

        public bool IsDragging() => _currentState == State.Dragging;
    }
}