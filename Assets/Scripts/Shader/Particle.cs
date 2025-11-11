using System.Runtime.InteropServices;
using UnityEngine;

namespace SPHSimulator.Shader
{
    [System.Serializable]
    [StructLayout(LayoutKind.Sequential, Size = ParticleStructConstants.ParticleStructSize)]
    public struct Particle
    {
        public float Pressure;
        public float Density; 
        public Vector3 CurrentForce;
        public Vector3 Velocity;
        public Vector3 Position;
    }
}