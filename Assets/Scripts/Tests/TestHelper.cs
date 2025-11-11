using SPHSimulator.Controls;
using SPHSimulator.Shader;
using SPHSimulator.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace SPHSimulator.Tests
{
    public static class TestHelper
    {
        public static void SetValueOfField<T>(string name, object obj, object value)
        {
            typeof(T).GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(obj, value);
        }

        public static SPH SetupSPH(GameObject testObject)
        {
            //Prevent awake from happening too early
            testObject.SetActive(false);     

            SPH sph = testObject.AddComponent<SPH>();
            TestShaderComponent testShaderHelper = Object.Instantiate(Resources.Load<GameObject>("TestObjects/TestShaderHelper")).GetComponent<TestShaderComponent>();
            SetValueOfField<SPH>("shader", sph, testShaderHelper.GetShader());
            SetValueOfField<SPH>("material", sph, new Material(UnityEngine.Shader.Find("Standard")));
            SetValueOfField<SPH>("obstacleManager", sph, testObject.AddComponent<ObstacleManager>());
            SetValueOfField<SPH>("particleMesh", sph, GameObject.CreatePrimitive(PrimitiveType.Sphere).GetComponent<MeshFilter>().mesh);

            ContainerVisualisation containerVisualization = testObject.AddComponent<ContainerVisualisation>();

            SetValueOfField<ContainerVisualisation>("ground", containerVisualization, new GameObject().transform);
            SetValueOfField<ContainerVisualisation>("sideX_1", containerVisualization, new GameObject().transform);
            SetValueOfField<ContainerVisualisation>("sideX_2", containerVisualization, new GameObject().transform);
            SetValueOfField<ContainerVisualisation>("sideZ_1", containerVisualization, new GameObject().transform);
            SetValueOfField<ContainerVisualisation>("sideZ_2", containerVisualization, new GameObject().transform);

            SetValueOfField<SPH>("containerVisualiser", sph, containerVisualization);

            testObject.SetActive(true);

            return sph;
        }

        public static FluidRayMarching SetupFluidRayMarchingFromGameObjectWithFluidRayMarchingComponent(GameObject testObject)
        {
            testObject.SetActive(false);

            FluidRayMarching rayMarching = testObject.GetComponent<FluidRayMarching>();
            SetValueOfField<FluidRayMarching>("lightSource", rayMarching, new GameObject("Light").AddComponent<Light>());
            SPH sph = SetupSPH(testObject);
            SetValueOfField<FluidRayMarching>("sph", rayMarching, sph);

            testObject.SetActive(true);

            return rayMarching;
        }

        public static void SetupMainUI(GameObject testObject)
        {
            MainUI mainUI = testObject.GetComponent<MainUI>();
            if (mainUI == null)
            {
                Debug.LogError("MainUI component not found on testObject.");
                return;
            }

            // Ensure UIDocument exists and has a test UXML
            UIDocument uiDocument = testObject.GetComponent<UIDocument>();
            if (uiDocument == null)
                uiDocument = testObject.AddComponent<UIDocument>();

            // Load mock/test UI from Resources
            uiDocument.visualTreeAsset = Resources.Load<VisualTreeAsset>("TestObjects/TestUIMock");
            if (uiDocument.visualTreeAsset == null)
            {
                Debug.LogError("TestUIMock.uxml not found in Resources/TestObjects/");
                return;
            }

            // Dependencies
            Camera testCamera = new GameObject("TestCamera").AddComponent<Camera>();
            SPH sph = SetupSPH(testObject);
            ObstacleManager obstacleManager = testObject.GetComponent<ObstacleManager>() ?? testObject.AddComponent<ObstacleManager>();
            FluidRayMarching fluidRay = testObject.AddComponent<FluidRayMarching>();
            FreeCameraController camController = testObject.AddComponent<FreeCameraController>();
            UINotificationManager notificationManager = testObject.AddComponent<UINotificationManager>();

            // Mock gizmos
            DirectionGizmoUI gizmoGravity = testObject.AddComponent<DirectionGizmoUI>();
            DirectionGizmoUI gizmoWind = testObject.AddComponent<DirectionGizmoUI>();

            // Inject all dependencies
            SetValueOfField<MainUI>("mainCamera", mainUI, testCamera);
            SetValueOfField<MainUI>("sph", mainUI, sph);
            SetValueOfField<MainUI>("obstacleManager", mainUI, obstacleManager);
            SetValueOfField<MainUI>("fluidRayMarching", mainUI, fluidRay);
            SetValueOfField<MainUI>("cameraController", mainUI, camController);
            SetValueOfField<MainUI>("directionGizmoGravity", mainUI, gizmoGravity);
            SetValueOfField<MainUI>("directionGizmoWind", mainUI, gizmoWind);
            SetValueOfField<MainUI>("notificationManager", mainUI, notificationManager);
        }
    }
}