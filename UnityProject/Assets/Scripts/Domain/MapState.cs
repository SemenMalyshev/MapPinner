using System.Collections.Generic;

namespace Domain
{
    public class MapState
    {
        private readonly Dictionary<PinId, PinEntity> _pins = new();
        public readonly MapId MapId;
        public IEnumerable<PinEntity> Pins => _pins.Values;

        public MapState(MapData data)
        {
            MapId = data.MapId;
            foreach (var pin in data.Pins)
                _pins[pin.Id] = pin;
        }

        public void AddPin(PinEntity pin) => _pins[pin.Id] = pin;

        public PinEntity GetPin(PinId id) => _pins.TryGetValue(id, out var pin) ? pin : null;

        public void RemovePin(PinId id) => _pins.Remove(id);

        public MapData ToData()
        {
            return new MapData
            {
                MapId = this.MapId,
                Pins = new List<PinEntity>(_pins.Values)
            };
        }
    }
}