using System;

namespace Domain
{
   public readonly struct PinId : IEquatable<PinId>
    {
        public Guid Value { get; }
        public PinId(Guid value) => Value = value;
        public static PinId NewId() => new(Guid.NewGuid());
        public bool Equals(PinId other) => Value.Equals(other.Value);
        public override bool Equals(object obj) => obj is PinId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();
    }
}