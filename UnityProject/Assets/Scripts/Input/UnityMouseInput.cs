using UnityEngine;

namespace Input
{
    public class UnityMouseInput : IMouseInput
    {
        public bool GetLeftMouseButtonDown() => UnityEngine.Input.GetMouseButtonDown(0);
        public bool GetLeftMouseButtonUp() => UnityEngine.Input.GetMouseButtonUp(0);
        public bool IsLeftMouseButtonPressed() => UnityEngine.Input.GetMouseButton(0);
        public bool IsRightMouseButtonPressed() => UnityEngine.Input.GetMouseButton(1);
        public Vector2 GetMousePosition() => UnityEngine.Input.mousePosition;
        public float GetMouseScrollDelta() => UnityEngine.Input.mouseScrollDelta.y;
    }
}