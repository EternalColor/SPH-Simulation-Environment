using SPHSimulator.Configuration;
using SPHSimulator.Controls;
using SPHSimulator.Shader;
using UnityEngine;
using UnityEngine.UIElements;

namespace SPHSimulator.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class DirectionGizmoUI : MonoBehaviour
    {
        private const float RotationSpeed = 1f;
        private const float SnapAngle = 15f;
        private const float MaxStrengthValue = 999;

        public Vector3 ForceDirection => forceDirection;

        public ExternalForceType ExternalForceType => externalForceType;

        public float Strength
        {
            get 
            {
                return strength;
            }
            set
            {
                strength = value;

                if(strengthFloatField != null)
                {
                    strengthFloatField.value = value;
                }
            }
        }

        [SerializeField] 
        private ExternalForceType externalForceType;
        [SerializeField] 
        private GameObject gameObjectGizmo;

        private VisualElement gizmoUI;
        private Label gizmoLabel;
        private FloatField strengthFloatField;
        private Toggle toggleEnabled;

        private Vector3 forceDirection;
        private Vector3 lastMousePosition;
        private bool isDragging;

        private SPH sph;
        private FreeCameraController cameraController;

        private string sphShaderVariableName;

        private float strength;

        private float lastStrength;

        public void Initialize(VisualElement root, SPH sph, FreeCameraController cameraController)
        {
            this.sph = sph;
            this.cameraController = cameraController;
            cameraController.OnCameraRotation += OnCameraRotation;

            VisualElement externalForcesContainer = GetExternalForcesContainer(root, externalForceType);
            sphShaderVariableName = GetShaderVariableName(externalForceType);

            InitializeUI(externalForcesContainer);
            InitializeGizmoCamera();

            //Register MoueUpEvent on root, because it mouse can be released anywhere
            root.RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        private VisualElement GetExternalForcesContainer(VisualElement root, ExternalForceType forceType)
        {
            return forceType switch
            {
                ExternalForceType.Gravity => root.Q<VisualElement>("ExternalForcesGravityContainer"),
                ExternalForceType.Wind => root.Q<VisualElement>("ExternalForcesWindContainer"),
                _ => null
            };
        }

        private string GetShaderVariableName(ExternalForceType forceType)
        {
            return forceType switch
            {
                ExternalForceType.Gravity => SPHComputeShaderVariableNames.Gravity,
                ExternalForceType.Wind => SPHComputeShaderVariableNames.Wind,
                _ => string.Empty
            };
        }

        private void InitializeUI(VisualElement container)
        {
            gizmoUI = container.Q<VisualElement>("GizmoArea");
            gizmoLabel = container.Q<Label>("LabelResultingForce");
            strengthFloatField = container.Q<FloatField>("FloatFieldStrength");
            toggleEnabled = container.Q<Toggle>("ToggleEnabled");

            gizmoUI.pickingMode = PickingMode.Position;
            gizmoUI.RegisterCallback<MouseDownEvent>(OnMouseDown);

            strengthFloatField.RegisterValueChangedCallback(changeEvent =>
            {
                if (changeEvent.newValue <= MaxStrengthValue)
                {
                    strength = changeEvent.newValue;
                }
                else
                {
                    strengthFloatField.SetValueWithoutNotify(strength);
                }

                UpdateForce();
            });

            toggleEnabled.RegisterValueChangedCallback(OnToggleEnabledChanged);
        }

        private void InitializeGizmoCamera()
        {
            var camera = CameraUtilities.CreateCamera("gizmoCamera");
            camera.transform.position = gameObjectGizmo.transform.position + new Vector3(0, 0, -2f);
            camera.transform.LookAt(gameObjectGizmo.transform);

            var renderTexture = CameraUtilities.RenderToTexture(camera);
            gizmoUI.style.backgroundImage = new StyleBackground(Background.FromRenderTexture(renderTexture));
        }

        private void OnDestroy()
        {
            if (cameraController != null)
                cameraController.OnCameraRotation -= OnCameraRotation;
        }

        private void OnCameraRotation(Quaternion rotationDelta)
        {
            ApplyWorldRotationToVisual();
            gameObjectGizmo.transform.rotation = Quaternion.Inverse(rotationDelta) * gameObjectGizmo.transform.rotation;
        }

        public void SetGizmoByConfiguration(ConfigurationSimulation config)
        {
            forceDirection = externalForceType switch
            {
                ExternalForceType.Gravity => config.GravityDirection.GetValueOrDefault(),
                ExternalForceType.Wind => config.WindDirection.GetValueOrDefault(),
                _ => Vector3.forward
            };

            Strength = externalForceType switch
            {
                ExternalForceType.Gravity => config.GravityStrength.GetValueOrDefault(),
                ExternalForceType.Wind => config.WindStrength.GetValueOrDefault(),
                _ => 0f
            };

            if (Strength > 0)
            {
                toggleEnabled.value = true;
            }

            ApplyWorldRotationToVisual();
            UpdateForce();
        }

        private void Update()
        {
            if (isDragging)
            {
                HandleDragging();
            }
        }

        private void OnMouseDown(MouseDownEvent e)
        {
            if (e.button == 0)
            {
                lastMousePosition = Input.mousePosition;
                isDragging = true;
            }
        }

        private void OnMouseUp(MouseUpEvent e)
        {
            if (e.button == 0 && isDragging)
            {
                isDragging = false;
                SnapDirectionToIncrement();
                ApplyWorldRotationToVisual();
                UpdateForce();
            }
        }

        private void HandleDragging()
        {
            if (!isDragging) return;

            Vector3 delta = Input.mousePosition - lastMousePosition;

            if (delta.sqrMagnitude > Mathf.Epsilon)
            {
                Vector3 cameraRight = cameraController.transform.right;
                Vector3 cameraUp = cameraController.transform.up;
                Vector3 rotationAxis = delta.y * cameraRight - delta.x * cameraUp;

                Quaternion deltaRotation = Quaternion.AngleAxis(delta.magnitude * RotationSpeed, rotationAxis.normalized);

                forceDirection = deltaRotation * forceDirection;

                ApplyWorldRotationToVisual();
                UpdateForce();
            }

            lastMousePosition = Input.mousePosition;
        }

        private void ApplyWorldRotationToVisual()
        {
            Quaternion cameraRotation = cameraController.transform.rotation;

            // Compute the rotation that would make the gizmo "look at" forceDirection
            Quaternion worldLookRotation = Quaternion.LookRotation(forceDirection, Vector3.up);

            // Compensate for the camera rotation (to keep direction stable visually)
            gameObjectGizmo.transform.rotation = Quaternion.Inverse(cameraRotation) * worldLookRotation;
        }

        private void SnapDirectionToIncrement()
        {

            Quaternion rotation = Quaternion.LookRotation(forceDirection.normalized, Vector3.up);
            Vector3 euler = rotation.eulerAngles;

            euler.x = Mathf.Round(euler.x / SnapAngle) * SnapAngle;
            euler.y = Mathf.Round(euler.y / SnapAngle) * SnapAngle;
            euler.z = Mathf.Round(euler.z / SnapAngle) * SnapAngle;

            forceDirection = Quaternion.Euler(euler) * Vector3.forward;

            ApplyWorldRotationToVisual();
            UpdateForce();
        }

        private void UpdateForce()
        {
            Vector3 force = forceDirection.normalized * Strength;
            gizmoLabel.text = $"Resulting Force\n{force}";
            sph.SetShaderFloats(sphShaderVariableName, force.x, force.y, force.z, 0f);
        }

        private void OnToggleEnabledChanged(ChangeEvent<bool> changeEvent)
        {
            if (changeEvent.newValue)
            {
                if (Strength <= 0f)
                {
                    Strength = lastStrength > 0f ? lastStrength : 1f;
                }
            }
            else
            {
                lastStrength = Strength;
                Strength = 0f;
            }

            strengthFloatField.SetEnabled(changeEvent.newValue);
            UpdateForce();
        }
    }
}