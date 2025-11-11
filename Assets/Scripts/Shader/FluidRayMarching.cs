using SPHSimulator.Configuration;
using UnityEngine;

namespace SPHSimulator.Shader
{
    public class FluidRayMarching : ShaderManager
    {
        public ComputeBuffer _particlesBuffer;

        [SerializeField]
        private Camera mainCamera;

        [SerializeField]
        private SPH sph;

        [Header("Parameters")]
        [SerializeField]
        private float viewRadius;

        [SerializeField]
        private float blendStrength;

        [SerializeField]
        private Color ambientLight;

        [SerializeField]
        private Light lightSource;

        private RenderTexture targetRenderTexture;

        private bool mustRender = false;

        public override void ResetSimulation()
        {
            mustRender = false;
        }

        private void Awake()
        {
            shader.SetVector(FluidRayMarchingShaderVariableNames.FluidColor, (Vector3)Presets.Color[Presets.KeyStart].Color.GetValueOrDefault());
        }

        private void BeginRender()
        {
            InitRenderTexture();
            InitializeShaderVariables();

            mustRender = true;
        }

        private void InitRenderTexture()
        {
            if (targetRenderTexture == null || targetRenderTexture.width != mainCamera.pixelWidth || targetRenderTexture.height != mainCamera.pixelHeight)
            {
                if (targetRenderTexture != null)
                {
                    targetRenderTexture.Release();
                }

                mainCamera.depthTextureMode = DepthTextureMode.Depth;

                targetRenderTexture = new RenderTexture(mainCamera.pixelWidth, mainCamera.pixelHeight, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
                targetRenderTexture.enableRandomWrite = true;
                targetRenderTexture.Create();
            }
        }

        private void InitializeShaderVariables()
        {
            shader.SetBuffer(0, "particles", sph.ParticlesBuffer);
            shader.SetInt("numParticles", sph.Particles.Length);
            shader.SetFloat("particleRadius", viewRadius);
            shader.SetFloat("blendStrength", blendStrength);
            shader.SetVector("_AmbientLight", ambientLight);
            shader.SetTextureFromGlobal(0, "_DepthTexture", "_CameraDepthTexture");
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (!mustRender)
            {
                BeginRender();
            }

            if (mustRender)
            {
                shader.SetVector("_Light", lightSource.transform.forward);
                
                shader.SetTexture(0, "Source", source);
                shader.SetTexture(0, "Destination", targetRenderTexture);
                shader.SetVector("_CameraPos", mainCamera.transform.position);
                shader.SetMatrix("_CameraToWorld", mainCamera.cameraToWorldMatrix);
                shader.SetMatrix("_CameraInverseProjection", mainCamera.projectionMatrix.inverse);

                int threadGroupsX = Mathf.CeilToInt(mainCamera.pixelWidth / 8.0f);
                int threadGroupsY = Mathf.CeilToInt(mainCamera.pixelHeight / 8.0f);
                shader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

                Graphics.Blit(targetRenderTexture, destination);
            }
        }
    }
}