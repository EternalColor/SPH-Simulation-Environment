using UnityEngine;

public class CameraUtilities : MonoBehaviour
{
    public static Camera CreateCamera(string name = "Camera")
    {
        GameObject snapshotCameraObject = new GameObject(name);
        Camera snapshotCamera = snapshotCameraObject.AddComponent<Camera>();
        snapshotCamera.clearFlags = CameraClearFlags.Color;
        snapshotCamera.backgroundColor = new Color(0, 0, 0, 0); // transparent bg
        snapshotCamera.orthographic = false;
        snapshotCamera.fieldOfView = 40;

        return snapshotCamera;
    }

    public static RenderTexture RenderToTexture(Camera snapshotCamera)
    {
        var renderTexture = new RenderTexture(256, 256, 16);
        snapshotCamera.targetTexture = renderTexture;
        snapshotCamera.Render();

        return renderTexture;
    }

}
