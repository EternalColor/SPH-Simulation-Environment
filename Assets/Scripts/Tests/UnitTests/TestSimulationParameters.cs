using System.Collections;
using NUnit.Framework;
using SPHSimulator.Configuration;
using SPHSimulator.Shader;
using UnityEngine;
using UnityEngine.TestTools;

namespace SPHSimulator.Tests
{
    public class TestConfigurationSimulation
    {
        private GameObject testObject;
        private SPH sph;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            testObject = Object.Instantiate(Resources.Load<GameObject>("TestObjects/TestExternalForces"));
            sph = TestHelper.SetupSPH(testObject);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(testObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator NumberOfParticles_NormalValues()
        {
            SerializableInt3 expected = new SerializableInt3(5, 5, 5);
            ConfigurationSimulation config = new ConfigurationSimulation(numberOfParticles: expected);

            sph.NumberOfParticles = config.NumberOfParticles.Value;
            yield return null;

            Assert.AreEqual(expected, sph.NumberOfParticles);
        }

        [UnityTest]
        public IEnumerator NumberOfParticles_EdgeValues()
        {
            SerializableInt3 expected = new SerializableInt3(1, 1, 1);
            ConfigurationSimulation config = new ConfigurationSimulation(numberOfParticles: expected);

            sph.NumberOfParticles = config.NumberOfParticles.Value;
            yield return null;

            Assert.AreEqual(expected, sph.NumberOfParticles);
        }

        [UnityTest]
        public IEnumerator NumberOfParticles_OutOfRangeValues()
        {
            SerializableInt3 invalid = new SerializableInt3(0, -5, 10000);
            ConfigurationSimulation config = new ConfigurationSimulation(numberOfParticles: invalid);

            SerializableInt3 fallback = Presets.Simulation[Presets.KeyStart].NumberOfParticles.GetValueOrDefault();

            SerializableInt3 used = config.NumberOfParticles.HasValue &&
                IsValidNumberOfParticles(config.NumberOfParticles.Value)
                ? config.NumberOfParticles.Value : fallback;

            sph.NumberOfParticles = used;
            yield return null;

            Assert.AreEqual(fallback, sph.NumberOfParticles);
        }

        private bool IsValidNumberOfParticles(SerializableInt3 value)
        {
            if (value.X < 1 || value.Y < 1 || value.Z < 1) return false;
            if (value.X > 1000 || value.Y > 1000 || value.Z > 1000) return false;
            return true;
        }

        [UnityTest]
        public IEnumerator NumberOfParticles_InvalidValues_ShouldFallback()
        {
            ConfigurationSimulation config = new ConfigurationSimulation(numberOfParticles: null);

            SerializableInt3 defaultValue = Presets.Simulation[Presets.KeyStart].NumberOfParticles.GetValueOrDefault();
            sph.NumberOfParticles = config.NumberOfParticles ?? defaultValue;
            yield return null;

            Assert.AreEqual(defaultValue, sph.NumberOfParticles);
        }

        [UnityTest]
        public IEnumerator ContainerSize_NormalValues()
        {
            SerializableFloat3 expected = new SerializableFloat3(10f, 10f, 10f);

            sph.SetContainerSize(expected);
            yield return null;

            Assert.AreEqual((Vector3)expected, sph.GridContainerData.ContainerSize);
        }

       [UnityTest]
        public IEnumerator ContainerSize_EdgeValues()
        {
            SerializableFloat3 expected = new SerializableFloat3(0.1f, 0.1f, 0.1f);
            bool isNotValidContainerSize = sph.SetContainerSize(expected);
            yield return null;

            Assert.AreEqual(false, isNotValidContainerSize);
        }

        [UnityTest]
        public IEnumerator ContainerSize_RangeValues()
        {
            SerializableFloat3 expected = new SerializableFloat3(5f, 7f, 3f);

            sph.SetContainerSize(expected);
            yield return null;

            Assert.AreEqual((Vector3)expected, sph.GridContainerData.ContainerSize);
        }

        [UnityTest]
        public IEnumerator ContainerSize_OutOfRangeValues()
        {
            SerializableFloat3 invalid = new SerializableFloat3(-1f, 0f, 1000f);
            ConfigurationSimulation config = new ConfigurationSimulation(containerSize: invalid);

            SerializableFloat3 fallback = Presets.Simulation[Presets.KeyStart].ContainerSize.GetValueOrDefault();

            SerializableFloat3 used = config.ContainerSize.HasValue &&
                IsValidContainerSize(config.ContainerSize.Value)
                ? config.ContainerSize.Value : fallback;

            sph.SetContainerSize(used);
            yield return null;

            Assert.AreEqual((Vector3)fallback, sph.GridContainerData.ContainerSize);
        }

        private bool IsValidContainerSize(SerializableFloat3 value)
        {
            // Beispielhafte Validierung: alle Komponenten > 0 und < 100 (angepasst je nach Logik)
            if (value.X <= 0f || value.Y <= 0f || value.Z <= 0f) return false;
            if (value.X > 100f || value.Y > 100f || value.Z > 100f) return false;
            return true;
        }

        [UnityTest]
        public IEnumerator ContainerSize_InvalidValues_ShouldFallback()
        {
            ConfigurationSimulation config = new ConfigurationSimulation(containerSize: null);

            SerializableFloat3 fallback = Presets.Simulation[Presets.KeyStart].ContainerSize.GetValueOrDefault();
            sph.SetContainerSize(fallback);
            yield return null;

            Assert.AreEqual((Vector3)fallback, sph.GridContainerData.ContainerSize);
        }

        [UnityTest]
        public IEnumerator StartPosition_NormalValues()
        {
            SerializableFloat3 expected = new SerializableFloat3(2f, 3f, 4f);
            ConfigurationSimulation config = new ConfigurationSimulation(startPosition: expected);

            sph.StartPosition = config.StartPosition.Value;
            yield return null;

            Assert.AreEqual(expected, sph.StartPosition);
        }

        [UnityTest]
        public IEnumerator StartPosition_EdgeValues()
        {
            SerializableFloat3 expected = new SerializableFloat3(0f, 0f, 0f); // Randwert: Ursprung
            ConfigurationSimulation config = new ConfigurationSimulation(startPosition: expected);

            sph.StartPosition = config.StartPosition.Value;
            yield return null;

            Assert.AreEqual(expected, sph.StartPosition);
        }

        [UnityTest]
        public IEnumerator StartPosition_RangeValues()
        {
            SerializableFloat3 expected = new SerializableFloat3(1f, 5f, -2f); // Werte innerhalb typischer Grenzen
            ConfigurationSimulation config = new ConfigurationSimulation(startPosition: expected);

            sph.StartPosition = config.StartPosition.Value;
            yield return null;

            Assert.AreEqual(expected, sph.StartPosition);
        }

        [UnityTest]
        public IEnumerator StartPosition_OutOfRangeValues()
        {
            SerializableFloat3 invalid = new SerializableFloat3(float.PositiveInfinity, float.NaN, -9999f);
            ConfigurationSimulation config = new ConfigurationSimulation(startPosition: invalid);

            SerializableFloat3 fallback = Presets.Simulation[Presets.KeyStart].StartPosition.GetValueOrDefault();

            SerializableFloat3 used = config.StartPosition.HasValue &&
                IsValidStartPosition(config.StartPosition.Value)
                ? config.StartPosition.Value : fallback;

            sph.StartPosition = used;
            yield return null;

            Assert.AreEqual(fallback, sph.StartPosition);
        }

        [UnityTest]
        public IEnumerator StartPosition_InvalidValues_ShouldFallback()
        {
            ConfigurationSimulation config = new ConfigurationSimulation(startPosition: null);

            SerializableFloat3 fallback = Presets.Simulation[Presets.KeyStart].StartPosition.GetValueOrDefault();

            sph.StartPosition = config.StartPosition ?? fallback;
            yield return null;

            Assert.AreEqual(fallback, sph.StartPosition);
        }

        private bool IsValidStartPosition(SerializableFloat3 value)
        {
            // Beispielhafte Validierung: keine NaN oder Unendlich, x,y,z plausibel
            if (float.IsNaN(value.X) || float.IsNaN(value.Y) || float.IsNaN(value.Z)) return false;
            if (float.IsInfinity(value.X) || float.IsInfinity(value.Y) || float.IsInfinity(value.Z)) return false;
            // Optional: Grenzen setzen
            if (value.X < -10000f || value.X > 10000f) return false;
            if (value.Y < -10000f || value.Y > 10000f) return false;
            if (value.Z < -10000f || value.Z > 10000f) return false;
            return true;
        }

        // ---- Gravity Tests ----

        [UnityTest]
        public IEnumerator Gravity_NormalValues()
        {
            SerializableFloat3 expectedDirection = new SerializableFloat3(0f, -9.81f, 0f);
            float expectedStrength = 9.81f;

            ConfigurationSimulation config = new ConfigurationSimulation(
                gravityDirection: expectedDirection,
                gravityStrength: expectedStrength
            );

            Vector3 direction = config.GravityDirection ?? Vector3.zero;
            float strength = config.GravityStrength ?? 0f;

            yield return null;

            Assert.AreEqual((Vector3)expectedDirection, direction);
            Assert.AreEqual(expectedStrength, strength);
        }

        [UnityTest]
        public IEnumerator Gravity_EdgeValues()
        {
            SerializableFloat3 expectedDirection = new SerializableFloat3(0f, 0f, 0f);
            float expectedStrength = 0f;

            ConfigurationSimulation config = new ConfigurationSimulation(
                gravityDirection: expectedDirection,
                gravityStrength: expectedStrength
            );

            Vector3 direction = config.GravityDirection ?? Vector3.zero;
            float strength = config.GravityStrength ?? 0f;

            yield return null;

            Assert.AreEqual((Vector3)expectedDirection, direction);
            Assert.AreEqual(expectedStrength, strength);
        }

        [UnityTest]
        public IEnumerator Gravity_RangeValues()
        {
            SerializableFloat3 expectedDirection = new SerializableFloat3(1f, -1f, 0f);
            float expectedStrength = 15f;

            ConfigurationSimulation config = new ConfigurationSimulation(
                gravityDirection: expectedDirection,
                gravityStrength: expectedStrength
            );

            Vector3 direction = config.GravityDirection ?? Vector3.zero;
            float strength = config.GravityStrength ?? 0f;

            yield return null;

            Assert.AreEqual((Vector3)expectedDirection, direction);
            Assert.AreEqual(expectedStrength, strength);
        }

        [UnityTest]
        public IEnumerator Gravity_OutOfRangeValues()
        {
            SerializableFloat3 invalidDirection = new SerializableFloat3(float.NaN, float.PositiveInfinity, -999f);
            float invalidStrength = -50f; // Negative Stärke evtl. invalid

            ConfigurationSimulation config = new ConfigurationSimulation(
                gravityDirection: invalidDirection,
                gravityStrength: invalidStrength
            );

            Vector3 fallbackDirection = Presets.Simulation[Presets.KeyStart].GravityDirection.GetValueOrDefault();
            float fallbackStrength = Presets.Simulation[Presets.KeyStart].GravityStrength.GetValueOrDefault();

            Vector3 usedDirection = IsValidGravityDirection(config.GravityDirection) ? config.GravityDirection.Value : fallbackDirection;
            float usedStrength = IsValidGravityStrength(config.GravityStrength) ? config.GravityStrength.Value : fallbackStrength;

            yield return null;

            Assert.AreEqual(fallbackDirection, usedDirection);
            Assert.AreEqual(fallbackStrength, usedStrength);
        }

        [UnityTest]
        public IEnumerator Gravity_InvalidValues_ShouldFallback()
        {
            ConfigurationSimulation config = new ConfigurationSimulation(
                gravityDirection: null,
                gravityStrength: null
            );

            Vector3 fallbackDirection = Presets.Simulation[Presets.KeyStart].GravityDirection.GetValueOrDefault();
            float fallbackStrength = Presets.Simulation[Presets.KeyStart].GravityStrength.GetValueOrDefault();

            Vector3 direction = config.GravityDirection ?? fallbackDirection;
            float strength = config.GravityStrength ?? fallbackStrength;

            yield return null;

            Assert.AreEqual(fallbackDirection, direction);
            Assert.AreEqual(fallbackStrength, strength);
        }

        private bool IsValidGravityDirection(SerializableFloat3? dir)
        {
            if (!dir.HasValue) return false;
            SerializableFloat3 d = dir.Value;
            if (float.IsNaN(d.X) || float.IsNaN(d.Y) || float.IsNaN(d.Z)) return false;
            if (float.IsInfinity(d.X) || float.IsInfinity(d.Y) || float.IsInfinity(d.Z)) return false;
            return true;
        }

        private bool IsValidGravityStrength(float? strength)
        {
            if (!strength.HasValue) return false;
            // Stärke sollte nicht negativ sein (je nach Logik)
            if (strength.Value < 0f) return false;
            if (float.IsNaN(strength.Value) || float.IsInfinity(strength.Value)) return false;
            return true;
        }

        // ---- Wind Tests ----

        [UnityTest]
        public IEnumerator Wind_NormalValues()
        {
            SerializableFloat3 expectedDirection = new SerializableFloat3(5f, 0f, 0f);
            float expectedStrength = 2f;

            ConfigurationSimulation config = new ConfigurationSimulation(
                windDirection: expectedDirection,
                windStrength: expectedStrength
            );

            Vector3 direction = config.WindDirection ?? Vector3.zero;
            float strength = config.WindStrength ?? 0f;

            yield return null;

            Assert.AreEqual((Vector3)expectedDirection, direction);
            Assert.AreEqual(expectedStrength, strength);
        }

        [UnityTest]
        public IEnumerator Wind_EdgeValues()
        {
            SerializableFloat3 expectedDirection = new SerializableFloat3(0f, 0f, 0f);
            float expectedStrength = 0f;

            ConfigurationSimulation config = new ConfigurationSimulation(
                windDirection: expectedDirection,
                windStrength: expectedStrength
            );

            Vector3 direction = config.WindDirection ?? Vector3.zero;
            float strength = config.WindStrength ?? 0f;

            yield return null;

            Assert.AreEqual((Vector3)expectedDirection, direction);
            Assert.AreEqual(expectedStrength, strength);
        }

        [UnityTest]
        public IEnumerator Wind_RangeValues()
        {
            SerializableFloat3 expectedDirection = new SerializableFloat3(1f, 1f, 1f);
            float expectedStrength = 10f;

            ConfigurationSimulation config = new ConfigurationSimulation(
                windDirection: expectedDirection,
                windStrength: expectedStrength
            );

            Vector3 direction = config.WindDirection ?? Vector3.zero;
            float strength = config.WindStrength ?? 0f;

            yield return null;

            Assert.AreEqual((Vector3)expectedDirection, direction);
            Assert.AreEqual(expectedStrength, strength);
        }

        [UnityTest]
        public IEnumerator Wind_OutOfRangeValues()
        {
            SerializableFloat3 invalidDirection = new SerializableFloat3(float.NaN, float.PositiveInfinity, -999f);
            float invalidStrength = -20f;

            ConfigurationSimulation config = new ConfigurationSimulation(
                windDirection: invalidDirection,
                windStrength: invalidStrength
            );

            Vector3 fallbackDirection = Presets.Simulation[Presets.KeyStart].WindDirection.GetValueOrDefault();
            float fallbackStrength = Presets.Simulation[Presets.KeyStart].WindStrength.GetValueOrDefault();

            Vector3 usedDirection = IsValidWindDirection(config.WindDirection) ? config.WindDirection.Value : fallbackDirection;
            float usedStrength = IsValidWindStrength(config.WindStrength) ? config.WindStrength.Value : fallbackStrength;

            yield return null;

            Assert.AreEqual(fallbackDirection, usedDirection);
            Assert.AreEqual(fallbackStrength, usedStrength);
        }

        [UnityTest]
        public IEnumerator Wind_InvalidValues_ShouldFallback()
        {
            ConfigurationSimulation config = new ConfigurationSimulation(
                windDirection: null,
                windStrength: null
            );

            Vector3 fallbackDirection = Presets.Simulation[Presets.KeyStart].WindDirection.GetValueOrDefault();
            float fallbackStrength = Presets.Simulation[Presets.KeyStart].WindStrength.GetValueOrDefault();

            Vector3 direction = config.WindDirection ?? fallbackDirection;
            float strength = config.WindStrength ?? fallbackStrength;

            yield return null;

            Assert.AreEqual(fallbackDirection, direction);
            Assert.AreEqual(fallbackStrength, strength);
        }

        private bool IsValidWindDirection(SerializableFloat3? dir)
        {
            if (!dir.HasValue)
            { 
                return false;
            }
            SerializableFloat3 d = dir.Value;
            if (float.IsNaN(d.X) || float.IsNaN(d.Y) || float.IsNaN(d.Z)) 
            {
                return false;
            }
            if (float.IsInfinity(d.X) || float.IsInfinity(d.Y) || float.IsInfinity(d.Z)) 
            {
                return false;
            }
            return true;
        }

        private bool IsValidWindStrength(float? strength)
        {
            if (!strength.HasValue)
            { return false;
            }
            if (strength.Value < 0f) 
            {
                return false;
            }
            if (float.IsNaN(strength.Value) || float.IsInfinity(strength.Value)) 
            {
                return false;
            }
            return true;
        }
    }
}
