using UnityEngine;
using UnityEngine.UIElements;

namespace SPHSimulator.UI
{
    public class ObstacleIconGenerator : MonoBehaviour
    {
        public UIDocument uiDocument;
        private Quaternion cubeRotation = Quaternion.Euler(325, 45, 150);
        private Vector3 spawnPoint = new Vector3(300, 300, 300);

        private void Start()
        {
            VisualElement root = uiDocument.rootVisualElement;

            Button sphereObstacle = root.Q<Button>(MainUIHandles.ButtonObstacleSphere);
            Button capsuleObstacle = root.Q<Button>(MainUIHandles.ButtonObstacleCapsule);
            Button cubeObstacle = root.Q<Button>(MainUIHandles.ButtonObstacleCube);

            Camera snapshotCamera = CameraUtilities.CreateCamera("SnapshotCamera");
            sphereObstacle.style.backgroundImage = new StyleBackground(CapturePrimitiveSnapshot(PrimitiveType.Sphere, snapshotCamera));
            capsuleObstacle.style.backgroundImage = new StyleBackground(CapturePrimitiveSnapshot(PrimitiveType.Capsule, snapshotCamera));
            cubeObstacle.style.backgroundImage = new StyleBackground(CapturePrimitiveSnapshot(PrimitiveType.Cube, snapshotCamera));

            DestroyImmediate(snapshotCamera.gameObject);
        }

        private Texture2D CapturePrimitiveSnapshot(PrimitiveType type, Camera snapshotCamera)
        {
            GameObject primitiveObject = CreatePrimitive(type);

            snapshotCamera.transform.position = primitiveObject.transform.position + new Vector3(0, 0, -3f);
            snapshotCamera.transform.LookAt(primitiveObject.transform);

            RenderTexture renderTexture = CameraUtilities.RenderToTexture(snapshotCamera);
            Texture2D texture = ConvertToTexture2D(renderTexture);

            snapshotCamera.targetTexture = null;
            DestroyImmediate(primitiveObject);
            DestroyImmediate(renderTexture);

            return texture;
        }

        public static Texture2D ConvertToTexture2D(RenderTexture renderTexture)
        {
            RenderTexture.active = renderTexture;
            var texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture.Apply();
            RenderTexture.active = null;

            return texture;
        }

        private GameObject CreatePrimitive(PrimitiveType type)
        {
            GameObject primitiveObject = GameObject.CreatePrimitive(type);
            Quaternion rotation = type == PrimitiveType.Cube ? cubeRotation : Quaternion.identity;
            primitiveObject.transform.position = spawnPoint;
            primitiveObject.transform.rotation = rotation;

            return primitiveObject;
        }
    }
}