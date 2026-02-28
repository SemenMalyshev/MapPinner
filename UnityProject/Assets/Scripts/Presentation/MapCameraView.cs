using UnityEngine;

namespace Presentation
{
    [RequireComponent(typeof(Camera))]
    public class MapCameraView : MonoBehaviour
    {
        private Camera _camera;
        public Camera Camera => _camera = _camera != null ? _camera : GetComponent<Camera>();
        public Vector2 ScreenToWorld(Vector2 screenPoint) => Camera.ScreenToWorldPoint(screenPoint);
        public Vector2 WorldToScreen(Vector2 worldPoint) => Camera.WorldToScreenPoint(worldPoint);
    }
}