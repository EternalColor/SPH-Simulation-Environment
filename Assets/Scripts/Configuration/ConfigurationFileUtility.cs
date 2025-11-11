using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace SPHSimulator.Configuration
{
    public static class ConfigurationFileUtility
    {
        public static bool TryWrite<T>(T objectToWrite) where T : IConfig
        {
            string path = GetConfigurationPath<T>();

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                string jsonText = JsonConvert.SerializeObject(objectToWrite);
                File.WriteAllText(path, jsonText);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static bool TryLoad<T>(out T configuration) where T : IConfig
        {
            configuration = default;

            string path = GetConfigurationPath<T>();

            try
            {
                string jsonText = File.ReadAllText(path);
                configuration = JsonConvert.DeserializeObject<T>(jsonText);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static string GetConfigurationPath<T>() where T : IConfig
        {
#if UNITY_EDITOR
            return $"{Application.dataPath}/SaveFiles/{typeof(T).Name}.json";
#else
            return $"{Application.persistentDataPath}/SaveFiles/{typeof(T).Name}.json";
#endif
        }
    }
}
