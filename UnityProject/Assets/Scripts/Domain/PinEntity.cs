using UnityEngine;

namespace Domain
{
    public class PinEntity
    {
        public PinId Id { get; }
        public Vector2 Position { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
        public string AudioPath { get; set; }

        public PinEntity(PinId id, Vector2 position, string title = "", string description = "", string imagePath = "", string audioPath = "")
        {
            Id = id;
            Position = position;
            Title = title;
            Description = description;
            ImagePath = imagePath;
            AudioPath = audioPath;
        }
    }
}