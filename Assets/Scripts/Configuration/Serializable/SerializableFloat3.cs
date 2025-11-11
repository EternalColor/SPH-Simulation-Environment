using System;
using Newtonsoft.Json;
using Unity.Mathematics;
using UnityEngine;

namespace SPHSimulator.Configuration
{
    /// <summary>
    /// This offers a way how to serialize the components of a <see cref="UnityEngine.Mathematics.float3"/>.
    /// It is needed because the normal implementetion of <see cref="UnityEngine.Mathematics.float3"/> is not serializable, 
    /// because it contains members which refer to its own type.
    /// </summary>
    public readonly struct SerializableFloat3 : IEquatable<SerializableFloat3>
    {
        public readonly float X;
        public readonly float Y;
        public readonly float Z;
        
        public SerializableFloat3(in float3 value)
        {
            X = value.x;
            Y = value.y;
            Z = value.z;
        }

        [JsonConstructor]
        public SerializableFloat3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public bool Equals(SerializableFloat3 other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }

        public static bool operator ==(SerializableFloat3 left, SerializableFloat3 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SerializableFloat3 left, SerializableFloat3 right)
        {
            return !(left == right);
        }

        public static implicit operator float3(in SerializableFloat3 value)
        {
            return new float3(value.X, value.Y, value.Z);
        }

        public static implicit operator Vector3(in SerializableFloat3 value)
        {
            return new Vector3(value.X, value.Y, value.Z);
        }

        /// <summary>
        /// Creates a <see cref="UnityEngine.Color"/> from a <see cref="SerializableFloat3"/> while neglecting the alpha channel parameter, so it will default to 1.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Color(in SerializableFloat3 value)
        {
            return new Color(value.X, value.Y, value.Z);
        }

        public static implicit operator SerializableFloat3(in float3 value)
        {
            return new SerializableFloat3(value);
        }

        public static implicit operator SerializableFloat3(in Vector3 value)
        {
            return new SerializableFloat3(value.x, value.y, value.z);
        }
    }
}