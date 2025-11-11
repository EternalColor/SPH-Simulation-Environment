using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SPHSimulator.Shader
{
    public class ObstacleManager : MonoBehaviour
    {
        private const float OBSTACLE_PREVIEW_SCALE = 0.6f;

        public Action<ComputeBuffer, int> OnSetObstaclesBuffer;
        public Action<ComputeBuffer> OnReleaseBuffer;

        public ComputeBuffer ObstacleBuffer => obstacleBuffer;

        private List<GameObject> instantiatedObstacles = new List<GameObject>();
        private List<ObstacleData> shaderObstacles = new List<ObstacleData>();
        private bool isObstaclePlacementActive;
        private GameObject obstaclePreview;
        private Func<Transform, ObstacleData> createObstacleDataFunction;

        private ComputeBuffer obstacleBuffer;
        private Vector3 spawnPoint = new Vector3(300, 300, 300);

        private void Update()
        {
            if (isObstaclePlacementActive && obstaclePreview != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
                if (groundPlane.Raycast(ray, out float distance))
                {
                    Vector3 hitPoint = ray.GetPoint(distance);
                    obstaclePreview.transform.position = new Vector3(hitPoint.x, obstaclePreview.transform.localScale.y / 2, hitPoint.z);
                }
            }
        }

        private void OnDestroy()
        {
            obstacleBuffer?.Release();
        }

        public void RegisterMouseClick(int mouseButton)
        {
            if (!isObstaclePlacementActive || obstaclePreview == null)
            {
                return;
            }

            switch (mouseButton)
            {
                case (int)PointerEventData.InputButton.Left:
                    ConfirmObstaclePlacement();
                    break;
                case (int)PointerEventData.InputButton.Right:
                    CancelObstaclePlacement();
                    break;
                default:
                    break;
            }
        }

        public void CreateBuffer()
        {
            var oldBuffer = obstacleBuffer;
            OnReleaseBuffer?.Invoke(oldBuffer);

            int obstaclesCount = Mathf.Max(1, shaderObstacles.Count);
            obstacleBuffer = new ComputeBuffer(obstaclesCount, Marshal.SizeOf<ObstacleData>());
            obstacleBuffer.SetData(shaderObstacles.Count > 0 ? shaderObstacles.ToArray() : new [] { default(ObstacleData) });
            OnSetObstaclesBuffer?.Invoke(obstacleBuffer, shaderObstacles.Count);
        }

        public void OnSelectObstacle(ObstacleType obstacleType)
        {
            if (isObstaclePlacementActive && obstaclePreview != null)
            {
                CancelObstaclePlacement();
            }

            switch (obstacleType)
            {
                case ObstacleType.Sphere:
                    CreateObstaclePreview(PrimitiveType.Sphere);
                    createObstacleDataFunction = ObstacleUtility.CreateSphere;
                    break;
                case ObstacleType.Cube:
                    CreateObstaclePreview(PrimitiveType.Cube);
                    createObstacleDataFunction = ObstacleUtility.CreateCube;
                    break;
                case ObstacleType.Capsule:
                    CreateObstaclePreview(PrimitiveType.Capsule);
                    createObstacleDataFunction = ObstacleUtility.CreateCapsule;
                    break;
            }
        }

        private void CreateObstaclePreview(PrimitiveType primitiveType)
        {
            obstaclePreview = GameObject.CreatePrimitive(primitiveType);
            obstaclePreview.transform.position = spawnPoint;
            obstaclePreview.transform.localScale = new Vector3(OBSTACLE_PREVIEW_SCALE, OBSTACLE_PREVIEW_SCALE, OBSTACLE_PREVIEW_SCALE);
            isObstaclePlacementActive = true;
        }

        public void RemoveAllObstacles()
        {
            shaderObstacles.Clear();
            CreateBuffer();

            for (int i = instantiatedObstacles.Count - 1; i >= 0; i--)
            {
                Destroy(instantiatedObstacles[i]);
            }
            instantiatedObstacles.Clear();

            CancelObstaclePlacement();
        }

        private void ConfirmObstaclePlacement()
        {
            SnapToGround(obstaclePreview);
            var obstacle = createObstacleDataFunction(obstaclePreview.transform);
            shaderObstacles.Add(obstacle);
            CreateBuffer();

            instantiatedObstacles.Add(obstaclePreview);
            isObstaclePlacementActive = false;
            obstaclePreview = null;
        }

        private void CancelObstaclePlacement()
        {
            Destroy(obstaclePreview);
            isObstaclePlacementActive = false;
        }

        private void SnapToGround(GameObject obstacle)
        {
            obstacle.transform.position = new Vector3(obstacle.transform.position.x, obstacle.transform.localScale.y / 2, obstacle.transform.position.z);
        }

        // TODO Delete
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.black;
            foreach (var obstacle in shaderObstacles)
            {
                if (obstacle.type == (uint)ObstacleType.Sphere)
                {
                    Gizmos.color = Color.yellowNice;
                    Gizmos.DrawSphere(obstacle.center, obstacle.size.x);
                }
                else if (obstacle.type == (uint)ObstacleType.Cube)
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawCube(obstacle.center, obstacle.size * 2f);
                }
                else if (obstacle.type == (uint)ObstacleType.Capsule)
                {
                    Gizmos.color = Color.green;
                    DrawCapsule(obstacle.center, obstacle.size, obstacle.offset);
                }
            }
        }

        private void DrawCapsule(Vector3 a, Vector3 b, float radius)
        {
            Gizmos.DrawLine(a, b);
            Gizmos.DrawSphere(a, radius);
            Gizmos.DrawSphere(b, radius);
        }
    }
}
