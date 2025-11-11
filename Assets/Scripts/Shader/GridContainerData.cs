using Unity.Mathematics;
using UnityEngine;

namespace SPHSimulator.Shader
{
    public readonly struct GridContainerData
    {
        public readonly int NumCells;
        public readonly Vector3 ContainerSize;
        public readonly int3 GridSize;

        public GridContainerData(in Vector3 containerSize, float cellSize)
        {
            ContainerSize = containerSize;
            GridSize = CalculateGridSize(containerSize, cellSize);
            NumCells = GridSize.x * GridSize.y * GridSize.z;
        }

        public bool IsValidSize(int particlesLength, float particleRadius)
        {
            float minRequiredVolume = GetMinimumRequiredVolume(particlesLength, particleRadius);
            float containerVolume = ContainerSize.x * ContainerSize.y * ContainerSize.z;

            return containerVolume >= minRequiredVolume;
        }

        public float GetMinimumRequiredVolume(int particlesLength, float particleRadius) 
            => particlesLength * Mathf.Pow(particleRadius * 2, 3);

        private static int3 CalculateGridSize(Vector3 containerSize, float cellSize)
        {
            Vector3 gridSizeFloat = containerSize / cellSize;
            return new int3(
                Mathf.Max(2, Mathf.CeilToInt(gridSizeFloat.x)),
                Mathf.Max(2, Mathf.CeilToInt(gridSizeFloat.y)),
                Mathf.Max(2, Mathf.CeilToInt(gridSizeFloat.z))
            );
        }
    }
}