using System.Collections.Generic;

namespace Domain
{
    public class MapData
    {
        public MapId MapId { get; set; }
        public List<PinEntity> Pins { get; set; }

        public MapData()
        {
            Pins = new List<PinEntity>();
        }
    }
}