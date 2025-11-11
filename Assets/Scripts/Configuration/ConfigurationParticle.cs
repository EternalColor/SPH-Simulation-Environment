using Newtonsoft.Json;

namespace SPHSimulator.Configuration
{
    public readonly struct ConfigurationParticle : IConfig
    {
        public readonly float? Viscosity;
        public readonly float? GasConstant;
        public readonly float? RestDensity;
        public readonly float? BoundDamping;

        [JsonConstructor]
        public ConfigurationParticle
        (
            float? viscosity = null,
            float? gasConstant = null,
            float? restDensity = null,
            float? boundDamping = null
        )
        {
            Viscosity = viscosity;
            GasConstant = gasConstant;
            RestDensity = restDensity;
            BoundDamping = boundDamping;
        }
    }
}