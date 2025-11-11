using System.Collections;
using System.IO;
using NUnit.Framework;
using SPHSimulator.Configuration;
using UnityEngine;
using UnityEngine.TestTools;

namespace SPHSimulator.Tests
{
    public class TestSaveAndLoad
    {
        private string saveDirectory;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            saveDirectory = Path.Combine(Application.dataPath, "SaveFiles");

            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
            }
        }

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            DeleteTestFiles();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            DeleteTestFiles();
            yield return null;
        }

        private void DeleteTestFiles()
        {
            string filePath = Path.Combine(saveDirectory, $"{nameof(ConfigurationColor)}.json");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        [UnityTest]
        public IEnumerator TryWrite_And_TryLoad_ShouldSucceed()
        {
            SerializableFloat3 expectedColor = new SerializableFloat3(0.25f, 0.5f, 0.75f);
            ConfigurationColor config = new ConfigurationColor(expectedColor);

            bool writeResult = ConfigurationFileUtility.TryWrite(config);
            Assert.IsTrue(writeResult);

            yield return null;

            ConfigurationColor loadedConfig;
            bool loadResult = ConfigurationFileUtility.TryLoad(out loadedConfig);

            Assert.IsTrue(loadResult);
            Assert.IsNotNull(loadedConfig.Color);
            Assert.AreEqual(expectedColor, loadedConfig.Color.Value);
        }

        [UnityTest]
        public IEnumerator TryLoad_ShouldFail_WhenFileMissing()
        {
            ConfigurationColor loadedConfig;
            bool loadResult = ConfigurationFileUtility.TryLoad(out loadedConfig);

            Assert.IsFalse(loadResult);
            Assert.AreEqual(default(ConfigurationColor), loadedConfig);
            yield return null;
        }

        [UnityTest]
        public IEnumerator TryLoad_ShouldFail_WhenJsonIsMalformed()
        {
            string path = Path.Combine(saveDirectory, $"{nameof(ConfigurationColor)}.json");

            File.WriteAllText(path, "{ invalid_json ");
            yield return null;

            ConfigurationColor loadedConfig;
            bool loadResult = ConfigurationFileUtility.TryLoad(out loadedConfig);

            Assert.IsFalse(loadResult);
            Assert.AreEqual(default(ConfigurationColor), loadedConfig);
        }

        [UnityTest]
        public IEnumerator TryWrite_ShouldNotFail_IfDirectoryMissing()
        {
            string path = Path.Combine(saveDirectory, $"{nameof(ConfigurationColor)}.json");

            if (Directory.Exists(saveDirectory))
            {
                Directory.Delete(saveDirectory, true);
            }

            ConfigurationColor config = new ConfigurationColor(new SerializableFloat3(1f, 1f, 1f));
            bool writeResult = ConfigurationFileUtility.TryWrite(config);

            Assert.IsTrue(writeResult);
            yield return null;
        }
    }
}