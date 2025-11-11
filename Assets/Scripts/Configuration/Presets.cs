using System.Collections.Generic;

namespace SPHSimulator.Configuration
{
    public static class Presets
    {
        public const string KeyStart = "Start";
        public const string Honey = "Honey";
        public const string Water = "Water";
        public const string Lava = "Lava";
        public const string Slime = "Slime";
        public const string Alcohol = "Alcohol";
        public const string MotorOil = "Motor oil";

        public static readonly IReadOnlyDictionary<string, ConfigurationColor> Color = new Dictionary<string, ConfigurationColor>()
        {
            {
                KeyStart,
                new ConfigurationColor
                (
                    color: new SerializableFloat3(0, 0, 1f)
                )
            },
            {
                Honey,
                new ConfigurationColor
                (
                    color: new SerializableFloat3(0.76f, 0.29f, 0.07f)
                )
            },
            {
                Water,
                new ConfigurationColor
                (
                    color: new SerializableFloat3(0, 0, 1f)
                )
            },
            {
                Lava,
                new ConfigurationColor
                (
                    color: new SerializableFloat3(0.57f, 0.02f, 0f)
                )
            },
            {
                Slime,
                new ConfigurationColor
                (
                    color: new SerializableFloat3(0.39f, 1f, 0.03f)
                )
            },
            {
                Alcohol,
                new ConfigurationColor
                (
                    color: new SerializableFloat3(1f, 1f, 1f)
                )
            },
            {
                MotorOil,
                new ConfigurationColor
                (
                    color: new SerializableFloat3(0.62f, 0.33f, 0.06f)
                )
            }
        };

        public static readonly IReadOnlyDictionary<string, ConfigurationParticle> Particle = new Dictionary<string, ConfigurationParticle>()
        {
            {
                KeyStart, 
                new ConfigurationParticle
                (
                    viscosity: 1,
                    gasConstant: 10f,
                    restDensity: 1000,
                    boundDamping: -0.4f
                )
            },
            {
                Water, 
                new ConfigurationParticle
                (
                    viscosity: 1,
                    gasConstant: 10f,
                    restDensity: 1000f,
                    boundDamping: -0.5f
                )
            },
            {
                Honey,
                new ConfigurationParticle
                (
                    viscosity: 30f,
                    gasConstant: 12f,
                    restDensity: 1400f,
                    boundDamping: -0.4f
                )
            },
            {
                Lava,
                new ConfigurationParticle
                (
                    viscosity: 80,
                    gasConstant: 10f,
                    restDensity: 3000f,
                    boundDamping: -0.5f
                )
            },
            {
                Slime,
                new ConfigurationParticle
                (
                    viscosity: 23,
                    gasConstant: 20f,
                    restDensity: 1000f,
                    boundDamping: -0.5f
                )
            },
            {
                Alcohol,
                new ConfigurationParticle
                (
                    viscosity: 1.07f,
                    gasConstant: 10f,
                    restDensity: 780,
                    boundDamping: -0.5f
                )
            },
            {
                MotorOil,
                new ConfigurationParticle
                (
                    viscosity: 1.07f,
                    gasConstant: 10f,
                    restDensity: 789,
                    boundDamping: -0.5f
                )
            },

        };

        public static readonly IReadOnlyDictionary<string, ConfigurationSimulation> Simulation = new Dictionary<string, ConfigurationSimulation>()
        {
            {
                KeyStart,
                new ConfigurationSimulation
                (
                    numberOfParticles: new SerializableInt3(10, 10, 10), 
                    containerSize: new SerializableFloat3(4, 10f, 4), 
                    startPosition: new SerializableFloat3(5f, 5f, 5f),
                    gravityDirection: new SerializableFloat3(0, -1, 0),
                    gravityStrength: 9.81f,
                    windDirection: new SerializableFloat3(-1, 0, 0),
                    windStrength: 0f
                )
            }
        };
    }
}