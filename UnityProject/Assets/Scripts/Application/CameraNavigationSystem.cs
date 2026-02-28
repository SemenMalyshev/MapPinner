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
        private float _minZoom = 2f;
        private float _targetOrthographicSize;

        public CameraNavigationSystem(IMouseInput input, MapCameraView cameraView, MapView mapView)
        {
            _input = input;
            _cameraView = cameraView;
            _mapView = mapView;
            _targetOrthographicSize = cameraView.Camera.orthographicSize;
        }

        public void Tick()
        {
            HandleZoom();
            HandlePan();
            ClampCamera();
        }

        private void HandleZoom()
        {
            var cam = _cameraView.Camera;
            var bounds = _mapView.Bounds;
            float maxZoom = Mathf.Min(bounds.size.x / cam.aspect, bounds.size.y) * 0.5f;
            float scroll = _input.GetMouseScrollDelta();

            if (Mathf.Abs(scroll) > 0.01f)
                _targetOrthographicSize = Mathf.Clamp(_cameraView.Camera.orthographicSize - scroll * 1.5f, _minZoom, maxZoom);

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
            float h = cam.orthographicSize;
            float w = h * cam.aspect;

            float maxZoom = Mathf.Min(bounds.size.x / cam.aspect, bounds.size.y) * 0.5f;
            float t = cam.orthographicSize / maxZoom;

            float maxX = bounds.max.x - maxZoom * cam.aspect * t;
            float minX = bounds.min.x + maxZoom * cam.aspect * t;
            float maxY = bounds.max.y - maxZoom * t;
            float minY = bounds.min.y + maxZoom * t;

            var pos = _cameraView.transform.position;
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            _cameraView.transform.position = pos;
        }
    }

}