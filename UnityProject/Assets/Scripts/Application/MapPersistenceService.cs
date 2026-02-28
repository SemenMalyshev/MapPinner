using Domain;

namespace Application
{
    public class MapPersistenceService
    {
        private readonly Infrastructure.IMapRepository _repository;

        public MapPersistenceService(Infrastructure.IMapRepository repository) => _repository = repository;
        public MapState Load(MapId mapId) => new(_repository.Load(mapId));
        public void Save(MapState mapState) => _repository.Save(mapState.ToData());
    }
}
