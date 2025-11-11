using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SPHSimulator.Configuration;
using SPHSimulator.Controls;
using SPHSimulator.Shader;
using SPHSimulator.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace SPHSimulator.Tests
{
    public class TestExternalForces
    {
        private GameObject testObject;
        private DirectionGizmoUI gizmoGravityUI, gizmoWindUI;
 
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            testObject = Object.Instantiate(Resources.Load<GameObject>("TestObjects/TestExternalForces"));

            GameObject testCameraGameObject = new GameObject("TestCamera");
            FreeCameraController cameraController = testCameraGameObject.AddComponent<FreeCameraController>();

            gizmoGravityUI = testObject.GetComponents<DirectionGizmoUI>().First(itm => itm.ExternalForceType == ExternalForceType.Gravity);
            SetupGizmo(gizmoGravityUI, cameraController);

            gizmoWindUI = testObject.GetComponents<DirectionGizmoUI>().First(itm => itm.ExternalForceType == ExternalForceType.Wind);
            SetupGizmo(gizmoWindUI, cameraController);

            SPH sph = TestHelper.SetupSPH(testObject);
            TestHelper.SetValueOfField<DirectionGizmoUI>("sph", gizmoGravityUI, sph);
            TestHelper.SetValueOfField<DirectionGizmoUI>("sph", gizmoWindUI, sph);

            TestHelper.SetValueOfField<DirectionGizmoUI>("toggleEnabled", gizmoGravityUI, new Toggle());
            TestHelper.SetValueOfField<DirectionGizmoUI>("toggleEnabled", gizmoWindUI, new Toggle());

            yield return null;
        }

        private void SetupGizmo(DirectionGizmoUI gizmoUI, FreeCameraController cameraController)
        {
            TestHelper.SetValueOfField<DirectionGizmoUI>("cameraController", gizmoUI, cameraController);
            TestHelper.SetValueOfField<DirectionGizmoUI>("gameObjectGizmo", gizmoUI, new GameObject("Game Object Gizmo"));
            TestHelper.SetValueOfField<DirectionGizmoUI>("gizmoLabel", gizmoUI, new Label("Gravity Gizmo Label"));
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(testObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Gravity_NormalValues()
        {
            Vector3 expectedDirection = new Vector3(1, 0, 0);
            float expectedStrength = 10f;

            ConfigurationSimulation config = new ConfigurationSimulation
            (
                gravityDirection: expectedDirection,
                gravityStrength: expectedStrength
            );

            gizmoGravityUI.SetGizmoByConfiguration(config);
            yield return null;

            Assert.AreEqual(expectedDirection.normalized, gizmoGravityUI.ForceDirection.normalized);
            Assert.AreEqual(expectedStrength, gizmoGravityUI.Strength);
        }

        [UnityTest]
        public IEnumerator Gravity_EdgeValues()
        {
            Vector3 expectedDirection = Vector3.forward;
            float expectedStrength = 0f;

            ConfigurationSimulation config = new ConfigurationSimulation
            (
                gravityDirection: expectedDirection,
                gravityStrength: expectedStrength
            );

            gizmoGravityUI.SetGizmoByConfiguration(config);
            yield return null;

            Assert.AreEqual(expectedDirection.normalized, gizmoGravityUI.ForceDirection.normalized);
            Assert.AreEqual(expectedStrength, gizmoGravityUI.Strength);
        }

        [UnityTest]
        public IEnumerator Gravity_OutOfRangeValues()
        {
            Vector3 expectedDirection = new Vector3(999999, 999999, 999999);
            float expectedStrength = float.MaxValue;

            ConfigurationSimulation config = new ConfigurationSimulation
            (
                gravityDirection: expectedDirection,
                gravityStrength: expectedStrength
            );

            gizmoGravityUI.SetGizmoByConfiguration(config);
            yield return null;

            Assert.AreEqual(expectedDirection.normalized, gizmoGravityUI.ForceDirection.normalized);
            Assert.AreEqual(expectedStrength, gizmoGravityUI.Strength);
        }

        [UnityTest]
        public IEnumerator Gravity_InvalidValues_ShouldFallback()
        {
            Vector3 expectedDirection = Vector3.zero;
            float expectedStrength = 0f;

            ConfigurationSimulation config = new ConfigurationSimulation
            (
                gravityDirection: null,
                gravityStrength: null
            );

            gizmoGravityUI.SetGizmoByConfiguration(config);
            yield return null;

            Assert.AreEqual(expectedDirection, gizmoGravityUI.ForceDirection);
            Assert.AreEqual(expectedStrength, gizmoGravityUI.Strength);
        }

        [UnityTest]
        public IEnumerator Wind_NormalValues()
        {
            Vector3 expectedDirection = new Vector3(0, 1, 0);
            float expectedStrength = 5f;

            ConfigurationSimulation config = new ConfigurationSimulation
            (
                windDirection: expectedDirection,
                windStrength: expectedStrength
            );

            gizmoWindUI.SetGizmoByConfiguration(config);
            yield return null;

            Assert.AreEqual(expectedDirection.normalized, gizmoWindUI.ForceDirection.normalized);
            Assert.AreEqual(expectedStrength, gizmoWindUI.Strength);
        }

        [UnityTest]
        public IEnumerator Wind_EdgeValues()
        {
            Vector3 expectedDirection = Vector3.up;
            float expectedStrength = 0f;

            ConfigurationSimulation config = new ConfigurationSimulation
            (
                windDirection: expectedDirection,
                windStrength: expectedStrength
            );

            gizmoWindUI.SetGizmoByConfiguration(config);
            yield return null;

            Assert.AreEqual(expectedDirection.normalized, gizmoWindUI.ForceDirection.normalized);
            Assert.AreEqual(expectedStrength, gizmoWindUI.Strength);
        }

        [UnityTest]
        public IEnumerator Wind_OutOfRangeValues()
        {
            Vector3 expectedDirection = new Vector3(-999999, 999999, -999999);
            float expectedStrength = float.MaxValue;

            ConfigurationSimulation config = new ConfigurationSimulation
            (
                windDirection: expectedDirection,
                windStrength: expectedStrength
            );

            gizmoWindUI.SetGizmoByConfiguration(config);
            yield return null;

            Assert.AreEqual(expectedDirection.normalized, gizmoWindUI.ForceDirection.normalized);
            Assert.AreEqual(expectedStrength, gizmoWindUI.Strength);
        }

        [UnityTest]
        public IEnumerator Wind_InvalidValues_ShouldFallback()
        {
            Vector3 expectedDirection = Vector3.zero;
            float expectedStrength = 0f;

            ConfigurationSimulation config = new ConfigurationSimulation
            (
                windDirection: null,
                windStrength: null
            );

            gizmoWindUI.SetGizmoByConfiguration(config);
            yield return null;

            Assert.AreEqual(expectedDirection, gizmoWindUI.ForceDirection);
            Assert.AreEqual(expectedStrength, gizmoWindUI.Strength);
        }
    }
}