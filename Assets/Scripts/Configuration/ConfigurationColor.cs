using Newtonsoft.Json;

namespace SPHSimulator.Configuration
{    
    public readonly struct ConfigurationColor : IConfig
    {
        /// <summary>
        /// The <see cref="Color"/> is represented as R-G-B float values (normalized between 0-1 like in Unity) for serialization.
        /// </summary>
        public readonly SerializableFloat3? Color;

        [JsonConstructor]
        public ConfigurationColor(SerializableFloat3? color = null)
        {
            Color = color;
        }
    }
}