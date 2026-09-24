using System;
using System.IO;
using System.Linq;
using Domain;
using Infrastructure;
using UnityEngine;

public static class MapRepositorySelfCheck
{
    public static void Run()
    {
        var id = new MapId("selfcheck-" + Guid.NewGuid().ToString("N"));
        var path = Path.Combine(UnityEngine.Application.persistentDataPath, "maps", id.Value + ".json");
        try
        {
            var repository = new JsonMapRepository();
            var data = new MapData { MapId = id };
            data.Pins.Add(new PinEntity(PinId.NewId(), new Vector2(1, 2), "Example", "Saved pin"));
            repository.Save(data);
            if (new JsonMapRepository().Load(id).Pins.Single().Title != "Example")
                throw new Exception("Map round trip failed.");

            repository.Save(data); // Creates the backup before replacing the main file.
            File.WriteAllText(path, "damaged");
            if (new JsonMapRepository().Load(id).Pins.Single().Title != "Example")
                throw new Exception("Backup recovery failed.");

            Debug.Log("Map repository self-check passed.");
        }
        finally
        {
            foreach (var suffix in new[] { "", ".bak", ".tmp", ".corrupt" })
                if (File.Exists(path + suffix)) File.Delete(path + suffix);
        }
    }
}
