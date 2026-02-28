using Domain;
using System.Collections.Generic;
using UnityEngine;

namespace Application
{
    public class PinCreationService
    {
        public PinEntity CreatePinAt(Vector2 position)
        {
            return new PinEntity(PinId.NewId(), position);
        }
    }
}