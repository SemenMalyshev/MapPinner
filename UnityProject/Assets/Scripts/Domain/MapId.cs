using System;

namespace Domain
{
    public readonly struct MapId : IEquatable<MapId>
    {
        public string Value { get; }
        public MapId(string value) => Value = value;
        public bool Equals(MapId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is MapId other && Equals(other);
        public override int GetHashCode() => Value != null ? Value.GetHashCode() : 0;
        public static implicit operator string(MapId id) => id.Value;
        public override string ToString() => Value;
    }
}