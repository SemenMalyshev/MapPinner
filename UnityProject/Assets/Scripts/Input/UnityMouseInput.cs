using UnityEngine;

namespace Input
{
    public class UnityMouseInput : IMouseInput
    {
        public bool GetLeftMouseButtonDown() => HasTouch ? UnityEngine.Input.GetTouch(0).phase == TouchPhase.Began : UnityEngine.Input.GetMouseButtonDown(0);
        public bool GetLeftMouseButtonUp() => HasTouch ? UnityEngine.Input.GetTouch(0).phase == TouchPhase.Ended : UnityEngine.Input.GetMouseButtonUp(0);
        public bool IsLeftMouseButtonPressed() => HasTouch
            ? UnityEngine.Input.GetTouch(0).phase != TouchPhase.Ended && UnityEngine.Input.GetTouch(0).phase != TouchPhase.Canceled
            : UnityEngine.Input.GetMouseButton(0);
        public bool IsRightMouseButtonPressed() => UnityEngine.Input.GetMouseButton(1);
        public Vector2 GetMousePosition() => HasTouch ? UnityEngine.Input.GetTouch(0).position : UnityEngine.Input.mousePosition;
        public float GetMouseScrollDelta() => UnityEngine.Input.mouseScrollDelta.y;

        private static bool HasTouch => UnityEngine.Input.touchCount > 0;
    }
}
