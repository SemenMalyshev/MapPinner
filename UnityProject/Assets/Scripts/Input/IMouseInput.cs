using UnityEngine;

namespace Input
{
    public interface IMouseInput
    {
        bool GetLeftMouseButtonDown();
        bool GetLeftMouseButtonUp();
        bool IsLeftMouseButtonPressed();
        bool IsRightMouseButtonPressed();
        Vector2 GetMousePosition();
        float GetMouseScrollDelta();
    }
}