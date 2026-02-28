using Domain;

namespace Infrastructure
{
    public interface IMapRepository
    {
        MapData Load(MapId mapId);
        void Save(MapData data);
    }
}