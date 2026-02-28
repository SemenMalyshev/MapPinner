using Domain;
using UnityEngine;

namespace Presentation
{
    public class PinViewFactory
    {
        private readonly PinView _pinPrefab;
        private readonly Transform _container;

        public PinViewFactory(PinView pinPrefab, Transform container)
        {
            _pinPrefab = pinPrefab;
            _container = container;
        }

        public PinView Create(PinEntity entity)
        {
            var pinView = Object.Instantiate(_pinPrefab, _container);
            pinView.Initialize(entity.Id, new Vector2(entity.Position.x, entity.Position.y));
            return pinView;
        }

        public void Destroy(PinView pinView) => Object.Destroy(pinView.gameObject);
    }
}