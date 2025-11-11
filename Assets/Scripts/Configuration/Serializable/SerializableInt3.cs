using System;
using Newtonsoft.Json;
using Unity.Mathematics;
using UnityEngine;

namespace SPHSimulator.Configuration
{
    /// <summary>
    /// This offers a way how to serialize the components of a <see cref="UnityEngine.Mathematics.int3"/>.
    /// It is needed because the normal implementetion of <see cref="UnityEngine.Mathematics.int3"/> is not serializable, 
    /// because it contains members which refer to its own type.
    /// </summary>
    public readonly struct SerializableInt3 : IEquatable<SerializableInt3>
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Z;

        public SerializableInt3(in int3 value)
        {
            X = value.x;
            Y = value.y;
            Z = value.z;
        }
        
        [JsonConstructor]
        public SerializableInt3(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public bool Equals(SerializableInt3 other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }

        public static bool operator ==(SerializableInt3 left, SerializableInt3 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SerializableInt3 left, SerializableInt3 right)
        {
            return !(left == right);
        }

        public static implicit operator SerializableInt3(in int3 value)
        {
            return new SerializableInt3(in value);
        }

        public static implicit operator int3(in SerializableInt3 value)
        {
            return new int3(value.X, value.Y, value.Z);
        }

        public static implicit operator Vector3(in SerializableInt3 value)
        {
            return new Vector3(value.X, value.Y, value.Z);
        }

        public static implicit operator Vector3Int(in SerializableInt3 value)
        {
            return new Vector3Int(value.X, value.Y, value.Z);
        }

        public static implicit operator SerializableInt3(in Vector3Int value)
        {
            return new SerializableInt3(value.x, value.y, value.z);
        }
    }
}