using System.Runtime.InteropServices;
using UnityEngine;

namespace SPHSimulator.Shader
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ObstacleData
    {
        public Vector3 center;     // For capsule: pointA
        public uint type;          // 0 = sphere, 1 = cube, 2 = capsule
        public Vector3 size;       // For capsule: pointB
        public float offset;       // For capsule: radius
    }
}