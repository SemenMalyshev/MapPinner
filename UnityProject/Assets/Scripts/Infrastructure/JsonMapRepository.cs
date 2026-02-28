using Domain;
using System.IO;
using UnityEngine;

namespace Infrastructure
{
    public class JsonMapRepository : IMapRepository
    {
        [System.Serializable]
        private class PinDto
        {
            public string id;
            public float x;
            public float y;
            public string title;
            public string description;
            public string imagePath;
            public string audioPath;
        }

        [System.Serializable]
        private class MapDataDto
        {
            public string mapId;
            public PinDto[] pins;
        }

        public MapData Load(MapId mapId)
        {
            var path = GetPathForMap(mapId);
            if (!File.Exists(path))
                return new MapData
                {
                    MapId = mapId,
                    Pins = new System.Collections.Generic.List<PinEntity>()
                };

            var json = File.ReadAllText(path);
            var dto = JsonUtility.FromJson<MapDataDto>(json);
            var mapData = new MapData { MapId = new MapId(dto.mapId) };

            foreach (var p in dto.pins)
            {
                mapData.Pins.Add(new PinEntity(
                    new PinId(System.Guid.Parse(p.id)),
                    new Vector2(p.x, p.y),
                    p.title, p.description, p.imagePath, p.audioPath
                ));
            }
            return mapData;
        }

        public void Save(MapData data)
        {
            var dto = new MapDataDto
            {
                mapId = data.MapId.Value,
                pins = new PinDto[data.Pins.Count]
            };

            for (int i = 0; i < data.Pins.Count; i++)
            {
                var pin = data.Pins[i];
                dto.pins[i] = new PinDto
                {
                    id = pin.Id.ToString(),
                    x = pin.Position.x,
                    y = pin.Position.y,
                    title = pin.Title,
                    description = pin.Description,
                    imagePath = pin.ImagePath,
                    audioPath = pin.AudioPath
                };
            }

            var path = GetPathForMap(data.MapId);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, JsonUtility.ToJson(dto, true));
            Debug.Log($"Map saved: {path}");
        }

        private string GetPathForMap(MapId mapId) =>
            Path.Combine(UnityEngine.Application.persistentDataPath, "maps", $"{mapId.Value}.json");
    }
}