using Newtonsoft.Json;

namespace SPHSimulator.Configuration
{
    public readonly struct ConfigurationSimulation : IConfig
    {
        public readonly SerializableInt3? NumberOfParticles;
        public readonly SerializableFloat3? ContainerSize;
        public readonly SerializableFloat3? StartPosition;
        public readonly SerializableFloat3? GravityDirection;
        public readonly float? GravityStrength;
        public readonly SerializableFloat3? WindDirection;
        public readonly float? WindStrength;

        [JsonConstructor]
        public ConfigurationSimulation
        (
            in SerializableInt3? numberOfParticles = null, 
            in SerializableFloat3? containerSize = null,
            in SerializableFloat3? startPosition = null, 
            in SerializableFloat3? gravityDirection = null, 
            float? gravityStrength = null, 
            in SerializableFloat3? windDirection = null,
            float? windStrength = null
        )
        {
            NumberOfParticles = numberOfParticles;
            ContainerSize = containerSize;
            StartPosition = startPosition;
            GravityDirection = gravityDirection;
            GravityStrength = gravityStrength;
            WindDirection = windDirection;
            WindStrength = windStrength;
        }
    }
}