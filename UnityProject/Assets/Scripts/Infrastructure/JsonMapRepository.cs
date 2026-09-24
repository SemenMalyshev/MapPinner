using Domain;
using System.IO;
using UnityEngine;

namespace Infrastructure
{
    public class JsonMapRepository : IMapRepository
    {
        private bool _loadedFromBackup;
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
                return new MapData { MapId = mapId };

            try { return ReadMap(path, mapId); }
            catch (System.Exception error)
            {
                Debug.LogWarning($"Could not load map {path}: {error.Message}");
                _loadedFromBackup = true;
            }

            try { return ReadMap(path + ".bak", mapId); }
            catch (System.Exception error)
            {
                Debug.LogError($"Could not load map backup: {error.Message}");
                return new MapData { MapId = mapId };
            }
        }

        private static MapData ReadMap(string path, MapId mapId)
        {
            var json = File.ReadAllText(path);
            var dto = JsonUtility.FromJson<MapDataDto>(json);
            if (dto == null || dto.pins == null || dto.mapId != mapId.Value)
                throw new InvalidDataException("Invalid map data");
            var mapData = new MapData { MapId = mapId };

            foreach (var p in dto.pins)
            {
                mapData.Pins.Add(new PinEntity(
                    new PinId(System.Guid.Parse(p.id)),
                    new Vector2(p.x, p.y),
                    p.title ?? "", p.description ?? "", p.imagePath ?? "", p.audioPath ?? ""
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
            var tempPath = path + ".tmp";
            File.WriteAllText(tempPath, JsonUtility.ToJson(dto, true));
            if (File.Exists(path))
                File.Copy(path, path + (_loadedFromBackup ? ".corrupt" : ".bak"), true);
            File.Copy(tempPath, path, true);
            File.Delete(tempPath);
            _loadedFromBackup = false;
            BrowserMedia.Sync();
            Debug.Log($"Map saved: {path}");
        }

        private string GetPathForMap(MapId mapId) =>
            Path.Combine(UnityEngine.Application.persistentDataPath, "maps", $"{mapId.Value}.json");
    }
}
