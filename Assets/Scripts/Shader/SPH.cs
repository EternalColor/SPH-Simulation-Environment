using SPHSimulator.Configuration;
using SPHSimulator.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace SPHSimulator.Shader
{
    public class SPH : ShaderManager
    {
        private const int THREAD_GROUP_SIZE = 64;
        private const int MAX_PARTICLES_PER_CELL = 256;
        private const float particleRadius = radius / 2f;
        private const float radius = 0.1f;
        private const float precomputedPoly6 = 315f / (64f * Mathf.PI * radius * radius * radius * radius * radius * radius * radius * radius * radius);

        private static readonly int shaderPropertyIDSize = UnityEngine.Shader.PropertyToID("_size");
        private static readonly int shaderPropertyIdParticlesBuffer = UnityEngine.Shader.PropertyToID("_particlesBuffer");

        public Particle[] Particles => particles;

        public GridContainerData GridContainerData => containerData;

        public SerializableInt3 NumberOfParticles { get; set; } = Presets.Simulation[Presets.KeyStart].NumberOfParticles.GetValueOrDefault();

        public SerializableFloat3 StartPosition { get; set; } = Presets.Simulation[Presets.KeyStart].StartPosition.GetValueOrDefault();

        public ComputeBuffer ParticlesBuffer;
        public event Action<Vector3> OnContainerSizeInvalid;
        public event Action<SerializableInt3, int> OnParticlesSpawned;
        public event Action<Vector3> OnContainerSizeUpdated;

        [Header("Particle Rendering")]
        [SerializeField]
        private Mesh particleMesh;

        [SerializeField]
        private float particleRenderSize = 16f;

        [SerializeField]
        private Material material;

        [Header("Simulation Parameters")]
        [SerializeField]
        private float timestep;

        [SerializeField]
        private ContainerVisualisation containerVisualiser;

        [SerializeField]
        private Particle[] particles;

        [SerializeField]
        private ObstacleManager obstacleManager;

        private ComputeBuffer bufferWithArgs;
        private ComputeBuffer gridParticleCountsBuffer;
        private ComputeBuffer gridParticleIndicesBuffer;

        private int particleGroupCount;
        private float cellSize;
        private GridContainerData containerData;

        private int clearKernel;
        private int assignKernel;
        private int densityAndPressureKernel;
        private int forcesKernel;
        private int integrationKernel;

        private void Awake()
        {
            timestep = 0.05f;
    
            cellSize = radius * 2f;  // has to be >= smoothing radius.

            clearKernel = shader.FindKernel("Clear");
            assignKernel = shader.FindKernel("Assign");
            densityAndPressureKernel = shader.FindKernel("DensityAndPressure");
            forcesKernel = shader.FindKernel("Forces");
            integrationKernel = shader.FindKernel("Integration");

            containerData = new GridContainerData(Presets.Simulation[Presets.KeyStart].ContainerSize.GetValueOrDefault(), cellSize);

            obstacleManager.OnSetObstaclesBuffer += SetShaderObstaclesBuffer;
            obstacleManager.OnReleaseBuffer += SafelyReleaseBuffer;
            obstacleManager.CreateBuffer();

            SetInitialShaderVariables();
            SetRenderingShaderVariables();
            StartSimulation();

            isRunning = true;
        }

        void OnDestroy()
        {
            ParticlesBuffer?.Release();
            bufferWithArgs?.Release();
            gridParticleCountsBuffer?.Release();
            gridParticleIndicesBuffer?.Release();
            obstacleManager.OnSetObstaclesBuffer -= SetShaderObstaclesBuffer;
            obstacleManager.OnReleaseBuffer -= SafelyReleaseBuffer;
        }

        public override void ResetSimulation()
        {
            StartSimulation();
        }

        private float CalculateParticleMass(float restDensity)
        {
            float kernelRadius = radius; // smoothing radius
            float kernelRadius2 = kernelRadius * kernelRadius;

            float spacing = particleRadius * 2f;

            float kernelSum = 0f;

            for (float x = -kernelRadius; x <= kernelRadius; x += spacing)
            {
                for (float y = -kernelRadius; y <= kernelRadius; y += spacing)
                {
                    for (float z = -kernelRadius; z <= kernelRadius; z += spacing)
                    {
                        Vector3 offset = new Vector3(x, y, z);
                        float dist2 = offset.sqrMagnitude;

                        if (dist2 <= kernelRadius2)
                        {
                            float diff = kernelRadius2 - dist2;
                            kernelSum += precomputedPoly6 * diff * diff * diff;
                        }
                    }
                }
            }
            return restDensity / kernelSum;
        }

        public bool SetContainerSize(in Vector3 newContainerSize)
        {
            var newData = new GridContainerData(newContainerSize, cellSize);
            if (!newData.IsValidSize(particles.Length, particleRadius))
            {
                OnContainerSizeInvalid?.Invoke(containerData.ContainerSize);
                return false;
            };
            containerData = newData;
            UpdateShaderContainerVariables();
            CreateGridBuffers(containerData.NumCells);
            containerVisualiser.UpdateVisualization(newContainerSize);
            
            return true;
        }

        public void RecalculateParticleMass(float newRestDensity)
        {
            float particleMass = CalculateParticleMass(newRestDensity);
            SetShaderParticleMass(particleMass);
            SetMaterialDensity(newRestDensity);
        } 

        public void SetShaderValuesByConfigurationParticle(ConfigurationParticle configurationParticle)
        {
            SetShaderFloat(SPHComputeShaderVariableNames.Viscosity, configurationParticle.Viscosity.GetValueOrDefault());
            SetShaderFloat(SPHComputeShaderVariableNames.GasConstant, configurationParticle.GasConstant.GetValueOrDefault());
            SetShaderFloat(SPHComputeShaderVariableNames.RestDensity, configurationParticle.RestDensity.GetValueOrDefault());
            SetShaderFloat(SPHComputeShaderVariableNames.BoundDamping, configurationParticle.BoundDamping.GetValueOrDefault());
            RecalculateParticleMass(configurationParticle.RestDensity.GetValueOrDefault());
        }

        private void SetShaderParticleMass(float particleMass)
        {
            shader.SetFloat(SPHComputeShaderVariableNames.ParticleMass, particleMass);
        }

        private void SetMaterialDensity(float densityRange)
        {
            material.SetFloat("_DensityRange", densityRange * 3);
        }

        private void SetInitialShaderVariables()
        {
            shader.SetFloat("radius", radius);
            shader.SetFloat("radius2", radius * radius);
            shader.SetFloat("pi", Mathf.PI);

            //Kernels defined in Particle-Based Fluid Simulation for Interactive Applications from Mathias Müller, David Charypar and Markus Gross
            shader.SetFloat("precomputedPoly6", precomputedPoly6);
            shader.SetFloat("precomputedSpikyGradient", 15f / (Mathf.PI * Mathf.Pow(radius, 6)));
            shader.SetFloat("precomputedViscosityLaplacian", 45f / (Mathf.PI * Mathf.Pow(radius, 6)));

            shader.SetFloat(SPHComputeShaderVariableNames.Viscosity, Presets.Particle[Presets.KeyStart].Viscosity.GetValueOrDefault());
            shader.SetFloat(SPHComputeShaderVariableNames.GasConstant, Presets.Particle[Presets.KeyStart].GasConstant.GetValueOrDefault());
            shader.SetFloat(SPHComputeShaderVariableNames.RestDensity, Presets.Particle[Presets.KeyStart].RestDensity.GetValueOrDefault());
            shader.SetFloat(SPHComputeShaderVariableNames.BoundDamping, Presets.Particle[Presets.KeyStart].BoundDamping.GetValueOrDefault());

            // Neighbour optimization
            shader.SetFloat("cellSize", cellSize);
            shader.SetInt("maxParticlesPerCell", MAX_PARTICLES_PER_CELL);

            SetShaderParticleMass(CalculateParticleMass(Presets.Particle[Presets.KeyStart].RestDensity.GetValueOrDefault()));
            shader.SetFloat("timestep", timestep);
        }

        private void SetRenderingShaderVariables()
        {
            material.SetFloat("_Radius", particleRadius);
            SetMaterialDensity(Presets.Particle[Presets.KeyStart].RestDensity.GetValueOrDefault());
            material.SetFloat(shaderPropertyIDSize, particleRenderSize);
        }

        private void StartSimulation()
        {
            particles = GetSpawnParticles();
            OnParticlesSpawned?.Invoke(NumberOfParticles, particles.Length);
            
            if (particles.Length == 0)
            {
                return;
            }

            particleGroupCount = Mathf.Max(1, Mathf.CeilToInt(particles.Length / (float)THREAD_GROUP_SIZE));

            CreateArgsBuffer(particles.Length);
            CreateParticlesBuffer(particles);

            // We keep the current size on restart
            if (!containerData.IsValidSize(particles.Length, radius))
            {
                var minRequired = containerData.GetMinimumRequiredVolume(particles.Length, radius);
                containerData = new GridContainerData(SplitIntoThreeFactors(Mathf.CeilToInt(minRequired)), cellSize);
                OnContainerSizeUpdated?.Invoke(containerData.ContainerSize);
            };
            SetContainerSize(containerData.ContainerSize);
            UpdateShaderVariables();

            material.SetBuffer(shaderPropertyIdParticlesBuffer, ParticlesBuffer);

            obstacleManager.CreateBuffer();
        }

        private Vector3 SplitIntoThreeFactors(int targetVolume)
        {
            float baseSize = Mathf.Pow(targetVolume, 1f / 3f); // cube root

            // keep the same for x and z, the remainder go to y
            int x = Mathf.CeilToInt(baseSize);
            int z = x;
            int y = Mathf.CeilToInt((float)targetVolume / (x * z));

            // Balance: If y ends up much bigger, adjust x and z upward
            while (x * z * y < targetVolume)
            {
                if (x <= z && x < y) x++;
                else if (z <= x && z < y) z++;
                else y++;
            }

            return new Vector3(x, y, z);
        }

        private void CreateArgsBuffer(int particleLength)
        {
            uint[] args =
            {
                particleMesh.GetIndexCount(0),
                (uint) particleLength,
                particleMesh.GetIndexStart(0),
                particleMesh.GetBaseVertex(0),
                0
            };

            ComputeBuffer oldBuffer = bufferWithArgs;
            bufferWithArgs = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            bufferWithArgs.SetData(args);

            SafelyReleaseBuffer(oldBuffer);
        }

        private void CreateParticlesBuffer(Particle[] particles)
        {
            ComputeBuffer oldBuffer = ParticlesBuffer;
            ParticlesBuffer = new ComputeBuffer(particles.Length, ParticleStructConstants.ParticleStructSize);
            ParticlesBuffer.SetData(particles);

            SafelyReleaseBuffer(oldBuffer);
        }

        private void CreateGridBuffers(int numCells)
        {
            ComputeBuffer oldCountsBuffer = gridParticleCountsBuffer;
            ComputeBuffer oldIndicesBuffer = gridParticleIndicesBuffer;

            gridParticleCountsBuffer = new ComputeBuffer(numCells, sizeof(uint));
            gridParticleIndicesBuffer = new ComputeBuffer(numCells * MAX_PARTICLES_PER_CELL, sizeof(uint));

            SafelyReleaseBuffer(oldCountsBuffer);
            SafelyReleaseBuffer(oldIndicesBuffer);
        }

        private bool IsValidBuffer(ComputeBuffer buffer) => buffer?.IsValid() ?? false;

        private void SafelyReleaseBuffer(ComputeBuffer buffer)
        {
            if (IsValidBuffer(buffer))
            {
                AsyncGPUReadback.Request(buffer, (request) =>
                {
                    buffer.Release();
                });
            }
        }

        public void SetParticleColor(Color color)
        {
            material.SetFloat("_Radius", particleRadius);
            material.SetColor("_Color", color);
        }

        private void SetShaderObstaclesBuffer(ComputeBuffer buffer, int obstacleCount)
        {
            shader.SetBuffer(integrationKernel, "_obstacles", buffer);
            shader.SetInt("obstacleCount", obstacleCount);
        }

        private void UpdateShaderContainerVariables()
        {
            SetShaderVector(SPHComputeShaderVariableNames.ContainerSize, containerData.ContainerSize);
            shader.SetInts("gridSize", containerData.GridSize.x, containerData.GridSize.y, containerData.GridSize.z);
            shader.SetInt("numCells", containerData.NumCells);
            shader.SetInt("maxParticlesPerCell", MAX_PARTICLES_PER_CELL);
        }

        private void UpdateShaderVariables()
        {
            shader.SetInt("particleLength", particles.Length);
            shader.SetFloat("cellSize", cellSize);
            shader.SetInt("maxParticlesPerCell", MAX_PARTICLES_PER_CELL);
        }

        private Particle[] GetSpawnParticles()
        {
            Particle[] spawnParticles = new Particle[NumberOfParticles.X * NumberOfParticles.Y * NumberOfParticles.Z];

            for (int x = 0; x < NumberOfParticles.X; x++)
            {
                for (int y = 0; y < NumberOfParticles.Y; y++)
                {
                    for (int z = 0; z < NumberOfParticles.Z; z++)
                    {
                        int index = x * NumberOfParticles.Y * NumberOfParticles.Z + y * NumberOfParticles.Z + z;

                        spawnParticles[index] = new Particle
                        {
                            Position = new Vector3(
                                Mathf.Clamp(x * 0.5f * UnityEngine.Random.value, particleRadius, containerData.ContainerSize.x - particleRadius),
                                Mathf.Clamp(y * 0.5f * UnityEngine.Random.value, particleRadius, containerData.ContainerSize.y - particleRadius),
                                Mathf.Clamp(z * 0.5f * UnityEngine.Random.value, particleRadius, containerData.ContainerSize.z - particleRadius))
                        };

                    }
                }
            }

            return spawnParticles;
        }

        private void DispatchClearKernel()
        {
            if (!IsValidBuffer(gridParticleCountsBuffer))
            {
                return;
            }

            int gridGroupCount = Mathf.Max(1, Mathf.CeilToInt(containerData.NumCells / (float)THREAD_GROUP_SIZE));
            shader.SetBuffer(clearKernel, "gridParticleCounts", gridParticleCountsBuffer);
            shader.Dispatch(clearKernel, gridGroupCount, 1, 1);
        }

        private void DispatchAssignKernel()
        {
            if (!IsValidBuffer(gridParticleCountsBuffer) || !IsValidBuffer(gridParticleIndicesBuffer) || !IsValidBuffer(ParticlesBuffer))
            {
                return;
            }

            shader.SetBuffer(assignKernel, "gridParticleCounts", gridParticleCountsBuffer);
            shader.SetBuffer(assignKernel, "gridParticleIndices", gridParticleIndicesBuffer);
            shader.SetBuffer(assignKernel, "_particles", ParticlesBuffer);
            shader.Dispatch(assignKernel, particleGroupCount, 1, 1);
        }

        private void DispatchComputeDensityKernel()
        {
            if (!IsValidBuffer(gridParticleCountsBuffer) || !IsValidBuffer(gridParticleIndicesBuffer) || !IsValidBuffer(ParticlesBuffer))
            {
                return;
            }
            shader.SetBuffer(densityAndPressureKernel, "gridParticleCounts", gridParticleCountsBuffer);
            shader.SetBuffer(densityAndPressureKernel, "gridParticleIndices", gridParticleIndicesBuffer);
            shader.SetBuffer(densityAndPressureKernel, "_particles", ParticlesBuffer);

          
            shader.Dispatch(densityAndPressureKernel, particleGroupCount, 1, 1);
        }

        private void DispatchComputeForcesKernel()
        {
            if (!IsValidBuffer(gridParticleCountsBuffer) || !IsValidBuffer(gridParticleIndicesBuffer) || !IsValidBuffer(ParticlesBuffer))
            {
                return;
            }

            shader.SetBuffer(forcesKernel, "gridParticleCounts", gridParticleCountsBuffer);
            shader.SetBuffer(forcesKernel, "gridParticleIndices", gridParticleIndicesBuffer);
            shader.SetBuffer(forcesKernel, "_particles", ParticlesBuffer);
            shader.Dispatch(forcesKernel, particleGroupCount, 1, 1);
        }

        private void DispatchIntegrateKernel()
        {
            if (!IsValidBuffer(obstacleManager.ObstacleBuffer) || !IsValidBuffer(ParticlesBuffer))
            {
                return;
            }

            shader.SetBuffer(integrationKernel, "_obstacles", obstacleManager.ObstacleBuffer);
            shader.SetBuffer(integrationKernel, "_particles", ParticlesBuffer);
            shader.Dispatch(integrationKernel, particleGroupCount, 1, 1);
        }


        private void FixedUpdate()
        {
            if (!isRunning)
            {
                return;
            }

            UpdateShaderVariables();

            DispatchClearKernel();
            DispatchAssignKernel();
            DispatchComputeDensityKernel();
            DispatchComputeForcesKernel();
            DispatchIntegrateKernel();

            material.SetFloat(shaderPropertyIDSize, particleRenderSize);
            material.SetBuffer(shaderPropertyIdParticlesBuffer, ParticlesBuffer);

            ParticlesBuffer.GetData(Particles);
        }

        private void Update()
        {
            DrawParticles();
        }

        private void DrawParticles()
        {
            Graphics.DrawMeshInstancedIndirect
            (
                particleMesh,
                0,
                material,
                new Bounds((Vector3)Presets.Simulation[Presets.KeyStart].ContainerSize / 2, (Vector3)Presets.Simulation[Presets.KeyStart].ContainerSize),
                bufferWithArgs,
                castShadows: ShadowCastingMode.Off
            );
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Vector3 containerSize = containerData.ContainerSize == Vector3.zero ? (Vector3)Presets.Simulation[Presets.KeyStart].ContainerSize : containerData.ContainerSize;
            Gizmos.DrawWireCube(containerSize / 2, containerSize);

            Gizmos.color = Color.darkRed;
            Gizmos.DrawSphere(StartPosition, 0.1f);         
        }
    }
}