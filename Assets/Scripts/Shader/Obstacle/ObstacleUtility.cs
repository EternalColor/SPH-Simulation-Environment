using UnityEngine;

namespace SPHSimulator.Shader
{
    public static class ObstacleUtility
    {
        public static ObstacleData CreateSphere(Transform transform)
        {
            Vector3 center = transform.position;
            float radius = transform.localScale.x / 2f;

            return new ObstacleData
            {
                center = center,
                type = (uint)ObstacleType.Sphere,
                size = new Vector3(radius, 0, 0)
            };
        }

        public static ObstacleData CreateCube(Transform transform)
        {
            Vector3 center = transform.position;
            Vector3 halfExtents = transform.localScale / 2f;

            return new ObstacleData
            {
                center = center,
                type = (uint)ObstacleType.Cube,
                size = halfExtents
            };
        }

        public static ObstacleData CreateCapsule(Transform transform)
        {
            var pos = transform.position;
            float radius = transform.localScale.x / 2f;
            Vector3 pointA = new Vector3(pos.x, pos.y + radius, pos.z);
            Vector3 pointB = new Vector3(pos.x, pos.y - radius, pos.z);

            return new ObstacleData
            {
                center = pointA,
                size = pointB,
                offset = radius,
                type = (uint)ObstacleType.Capsule
            };
        }
    }
}
