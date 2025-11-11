
using UnityEngine;

namespace SPHSimulator.Shader
{
    public abstract class ShaderManager : MonoBehaviour
    {
        public ComputeShader Shader => shader;

        protected bool isRunning = false;

        [Header("Compute Shader")]
        [SerializeField]
        protected ComputeShader shader;

        public abstract void ResetSimulation();

        public void ToggleSimulationRunning()
        {
            isRunning = !isRunning;
        }

        public void SetShaderFloat(string key, float value)
        {
            shader.SetFloat(key, value);
        }

        public void SetShaderFloats(string key, params float[] floats)
        {
            shader.SetFloats(key, floats);
        }

        public void SetShaderVector(string key, Vector4 value)
        {
            shader.SetVector(key, value);
        }
    }
}