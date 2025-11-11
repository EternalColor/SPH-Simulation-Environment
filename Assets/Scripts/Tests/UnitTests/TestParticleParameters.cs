using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using SPHSimulator.Shader;
using SPHSimulator.Configuration;

namespace SPHSimulator.Tests
{
    public class TestParticleParamaters
    {
        private GameObject testObject;
        private SPH sph;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            testObject = new GameObject("SPH_TestObject");

            testObject.SetActive(false);           

            sph = TestHelper.SetupSPH(testObject);

            testObject.SetActive(true);

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(testObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ParticleParameters_NormalValues()
        {
            float expectedViscosity = 0.1f;
            float expectedGasConstant = 2000f;
            float expectedRestDensity = 1000f;
            float expectedBoundDamping = 0.5f;

            ConfigurationParticle config = new ConfigurationParticle
            (
                viscosity: expectedViscosity,
                gasConstant: expectedGasConstant,
                restDensity: expectedRestDensity,
                boundDamping: expectedBoundDamping
            );

            sph.SetShaderValuesByConfigurationParticle(config);

            yield return null;
            Assert.Pass();
        }

        [UnityTest]
        public IEnumerator ParticleParameters_EdgeValues()
        {
            float expectedViscosity = 0f;
            float expectedGasConstant = float.MaxValue;
            float expectedRestDensity = 0f;
            float expectedBoundDamping = 1f;

            ConfigurationParticle config = new ConfigurationParticle
            (
                viscosity: expectedViscosity,
                gasConstant: expectedGasConstant,
                restDensity: expectedRestDensity,
                boundDamping: expectedBoundDamping
            );

            sph.SetShaderValuesByConfigurationParticle(config);

            yield return null;
            Assert.Pass();
        }

        [UnityTest]
        public IEnumerator ParticleParameters_OutOfRangeValues()
        {
            float expectedViscosity = float.MaxValue;
            float expectedGasConstant = float.MaxValue;
            float expectedRestDensity = float.MaxValue;
            float expectedBoundDamping = float.MaxValue;

            ConfigurationParticle config = new ConfigurationParticle
            (
                viscosity: expectedViscosity,
                gasConstant: expectedGasConstant,
                restDensity: expectedRestDensity,
                boundDamping: expectedBoundDamping
            );

            sph.SetShaderValuesByConfigurationParticle(config);

            yield return null;
            Assert.Pass();
        }

        [UnityTest]
        public IEnumerator ParticleParameters_DefaultValues()
        {
            float expectedViscosity = default;
            float expectedGasConstant = default;
            float expectedRestDensity = default;
            float expectedBoundDamping = default;


            ConfigurationParticle config = new ConfigurationParticle
            (
                viscosity: expectedViscosity,
                gasConstant: expectedGasConstant,
                restDensity: expectedRestDensity,
                boundDamping: expectedBoundDamping
            );

            sph.SetShaderValuesByConfigurationParticle(config);

            yield return null;
            Assert.Pass();
        }
    }
}