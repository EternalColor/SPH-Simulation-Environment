using UnityEngine;

namespace SPHSimulator.Tests
{
    public class TestShaderComponent : MonoBehaviour
    {   
        public ComputeShader GetShader() => sphComputeShader;

        [SerializeField] private ComputeShader sphComputeShader;

    }
}