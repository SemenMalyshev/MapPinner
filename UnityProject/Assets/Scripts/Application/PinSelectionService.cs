using Domain;
using System.Collections.Generic;
using UnityEngine;

namespace Application
{
    public class PinSelectionService
    {
        public delegate Vector2 WorldToScreenConverter(Vector2 worldPoint);

        public PinEntity FindPinAtScreenPosition(Vector2 screenPoint, IEnumerable<PinEntity> pins, WorldToScreenConverter converter, float tolerance)
        {
            PinEntity closest = null;
            float minDist = float.MaxValue;

            foreach (var pin in pins)
            {
                var screenPos = converter(pin.Position);
                float dist = Vector2.Distance(screenPoint, screenPos);
                if (dist < tolerance && dist < minDist)
                {
                    minDist = dist;
                    closest = pin;
                }
            }
            return closest;
        }
    }
}