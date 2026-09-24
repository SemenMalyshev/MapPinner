using Input;
using Presentation;
using UnityEngine;

namespace Application
{
    public class CameraNavigationSystem
    {
        private const float ZoomSpeed = 10f;

        private readonly IMouseInput _input;
        private readonly MapCameraView _cameraView;
        private readonly MapView _mapView;
        private Vector3 _lastMouseWorldPos;
        private const float MinZoom = 2f;
        private float _targetOrthographicSize;

        public CameraNavigationSystem(IMouseInput input, MapCameraView cameraView, MapView mapView)
        {
            _input = input;
            _cameraView = cameraView;
            _mapView = mapView;
            _targetOrthographicSize = cameraView.Camera.orthographicSize;
        }

        public void Tick(bool pinIsPressed)
        {
            if (UnityEngine.Input.touchCount > 0)
                HandleTouchNavigation(pinIsPressed);
            HandleZoom();
            if (UnityEngine.Input.touchCount == 0) HandlePan();
            ClampCamera();
        }

        private void HandleTouchNavigation(bool pinIsPressed)
        {
            if (UnityEngine.Input.touchCount == 2)
            {
                var first = UnityEngine.Input.GetTouch(0);
                var second = UnityEngine.Input.GetTouch(1);
                float currentDistance = Vector2.Distance(first.position, second.position);
                float previousDistance = Vector2.Distance(first.position - first.deltaPosition, second.position - second.deltaPosition);
                if (currentDistance > 1f && previousDistance > 1f)
                    _targetOrthographicSize = Mathf.Clamp(_targetOrthographicSize * previousDistance / currentDistance, MinZoom, GetMaxZoom());
            }
            else if (UnityEngine.Input.touchCount == 1 && !pinIsPressed)
            {
                var touch = UnityEngine.Input.GetTouch(0);
                if (touch.phase != TouchPhase.Moved) return;
                var before = _cameraView.ScreenToWorld(touch.position - touch.deltaPosition);
                var after = _cameraView.ScreenToWorld(touch.position);
                _cameraView.transform.position += (Vector3)(before - after);
            }
        }

        private float GetMaxZoom()
        {
            var bounds = _mapView.Bounds;
            return Mathf.Max(MinZoom, Mathf.Min(bounds.size.x / _cameraView.Camera.aspect, bounds.size.y) * 0.5f);
        }

        private void HandleZoom()
        {
            var cam = _cameraView.Camera;
            float scroll = _input.GetMouseScrollDelta();

            if (Mathf.Abs(scroll) > 0.01f)
                _targetOrthographicSize = Mathf.Clamp(cam.orthographicSize - scroll * 1.5f, MinZoom, GetMaxZoom());

            _cameraView.Camera.orthographicSize = Mathf.MoveTowards(_cameraView.Camera.orthographicSize, _targetOrthographicSize, Time.deltaTime * ZoomSpeed);
        }

        private void HandlePan()
        {
            if (!_input.IsRightMouseButtonPressed()) return;

            var mousePos = _input.GetMousePosition();
            var currentWorldPos = (Vector3)_cameraView.ScreenToWorld(mousePos);

            if (UnityEngine.Input.GetMouseButtonDown(1))
            {
                _lastMouseWorldPos = currentWorldPos;
                return;
            }

            var delta = _lastMouseWorldPos - currentWorldPos;
            _cameraView.transform.position += delta;
            _lastMouseWorldPos = _cameraView.ScreenToWorld(mousePos);
        }

        private void ClampCamera()
        {
            var cam = _cameraView.Camera;
            var bounds = _mapView.Bounds;
            float halfWidth = cam.orthographicSize * cam.aspect;
            float halfHeight = cam.orthographicSize;

            var pos = _cameraView.transform.position;
            pos.x = halfWidth >= bounds.extents.x
                ? bounds.center.x
                : Mathf.Clamp(pos.x, bounds.min.x + halfWidth, bounds.max.x - halfWidth);
            pos.y = halfHeight >= bounds.extents.y
                ? bounds.center.y
                : Mathf.Clamp(pos.y, bounds.min.y + halfHeight, bounds.max.y - halfHeight);
            _cameraView.transform.position = pos;
        }
    }

}
