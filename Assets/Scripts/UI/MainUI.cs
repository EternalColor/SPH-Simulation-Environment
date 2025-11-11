using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SPHSimulator.Configuration;
using SPHSimulator.Controls;
using SPHSimulator.Shader;
using UnityEngine;
using UnityEngine.UIElements;

namespace SPHSimulator.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class MainUI : MonoBehaviour
    {
        [SerializeField]
        private SPH sph;

        [SerializeField]
        private ObstacleManager obstacleManager;

        [SerializeField]
        private FluidRayMarching fluidRayMarching;

        [SerializeField]
        private FreeCameraController cameraController;

        [SerializeField]
        private DirectionGizmoUI directionGizmoGravity;

        [SerializeField]
        private DirectionGizmoUI directionGizmoWind;

        [SerializeField]
        private UINotificationManager notificationManager;

        private StartPauseButton startPause;

        private Button reset, save, load;
        
        private Button cubeObstacle, sphereObstacle, capsuleObstacle, deleteAllObstacles;

        private VisualElement viewport, root, presets;

        private Label actionNotification, informationLabel, particlesCount;

        private HSVColorPicker colorPickerFluid;

        private Slider viscosity, gasConstant, restDensity, boundDamping;

        private Vector3IntField numberOfParticles;

        private Vector3Field containerSize, startPosition;

        private Toggle toggleFluidRaymarching;

        private string currentFluidPresetKey = Presets.KeyStart;

        public event Action<bool> OnFluidRaymarchingChange;

        private void OnEnable()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
           
            directionGizmoGravity.Initialize(root, sph, cameraController);
            directionGizmoWind.Initialize(root, sph, cameraController);
            notificationManager.Initialize(root);

            InitializeElements();
            CreateFluidPresetButtons();
            RegisterCallbacks();
            SetInitialValues();

            cameraController.OnCursorLocked += OnCursorLocked;
            cameraController.OnCursorUnlocked += OnCursorUnlocked;

            sph.OnContainerSizeInvalid += OnContainerSizeInvalid;
            sph.OnContainerSizeUpdated += OnContainerSizeUpdated;
            sph.OnParticlesSpawned += OnParticlesSpawned;
        }

        private void OnDisable()
        {
            cameraController.OnCursorLocked -= OnCursorLocked;
            cameraController.OnCursorUnlocked -= OnCursorUnlocked;

            sph.OnContainerSizeInvalid -= OnContainerSizeInvalid;
            sph.OnContainerSizeUpdated -= OnContainerSizeUpdated;
            sph.OnParticlesSpawned -= OnParticlesSpawned;
        }

        private void InitializeElements()
        {
            viewport = root.Q<VisualElement>(MainUIHandles.ContainerCenter);
            startPause = root.Q<StartPauseButton>(MainUIHandles.ButtonStartPause);
            reset = root.Q<Button>(MainUIHandles.ButtonReset);
            save = root.Q<Button>(MainUIHandles.ButtonSave);
            load = root.Q<Button>(MainUIHandles.ButtonLoad);

            sphereObstacle = root.Q<Button>(MainUIHandles.ButtonObstacleSphere);
            capsuleObstacle = root.Q<Button>(MainUIHandles.ButtonObstacleCapsule);
            cubeObstacle = root.Q<Button>(MainUIHandles.ButtonObstacleCube);
            deleteAllObstacles = root.Q<Button>(MainUIHandles.ButtonDeleteAllObstacles);

            actionNotification = root.Q<Label>(MainUIHandles.LabelActionNotifications);

            colorPickerFluid = root.Q<HSVColorPicker>(MainUIHandles.ColorPickerFluid);
            
            viscosity = root.Q<Slider>(MainUIHandles.SliderViscosity);
            gasConstant = root.Q<Slider>(MainUIHandles.SliderGasConstant);
            restDensity = root.Q<Slider>(MainUIHandles.SliderRestDensity);
            boundDamping = root.Q<Slider>(MainUIHandles.SliderBoundDamping);

            numberOfParticles = root.Q<Vector3IntField>(MainUIHandles.Vector3IntNumberOfParticles);
            particlesCount = root.Q<Label>(MainUIHandles.LabelParticlesCount);
            containerSize = root.Q<Vector3Field>(MainUIHandles.Vector3ContainerSize);
            startPosition = root.Q<Vector3Field>(MainUIHandles.Vector3StartPosition);

            informationLabel = root.Q<Label>(MainUIHandles.LabelInformation);
            toggleFluidRaymarching = root.Q<Toggle>(MainUIHandles.ToggleFluidRaymarching);

            presets = root.Q<VisualElement>(MainUIHandles.ContainerPresets);
        }

        private void RegisterCallbacks()
        {
            RegisterActionCallbacks();
            RegisterFluidColorCallbacks();
            RegisterParticlePropertiesCallbacks();
            RegisterSimulationPropertiesCallback();
            RegisterObstacleCallbacks();

            viewport.RegisterCallback<MouseDownEvent>(OnMouseDownOnViewport);
            toggleFluidRaymarching.RegisterValueChangedCallback(evt =>
            {
                fluidRayMarching.enabled = evt.newValue;
            });
        }


        private void RegisterActionCallbacks()
        {
            startPause.RegisterCallback<ClickEvent>(OnClickStartStopButton);
            reset.RegisterCallback<ClickEvent>(OnClickResetButton);
            save.RegisterCallback<ClickEvent>(OnClickSaveButton);
            load.RegisterCallback<ClickEvent>(OnClickLoadButton);
        }
        private void RegisterObstacleCallbacks()
        {
            cubeObstacle.RegisterCallback<ClickEvent, ObstacleType>(OnClickObstacle, ObstacleType.Cube);
            sphereObstacle.RegisterCallback<ClickEvent, ObstacleType>(OnClickObstacle, ObstacleType.Sphere);
            capsuleObstacle.RegisterCallback<ClickEvent, ObstacleType>(OnClickObstacle, ObstacleType.Capsule);
            deleteAllObstacles.RegisterCallback<ClickEvent>(OnClickDeleteAllObstacles);
        }
        private void OnMouseDownOnViewport(MouseDownEvent mouseDownEvent)
        {
            obstacleManager.RegisterMouseClick(mouseDownEvent.button);
        }

        private void OnClickObstacle(ClickEvent clickEvent, ObstacleType obstacleType)
        {
            obstacleManager.OnSelectObstacle(obstacleType);
        }

        private void OnClickDeleteAllObstacles(ClickEvent clickEvent)
        {
            obstacleManager.RemoveAllObstacles();
        }

        private void OnClickStartStopButton(ClickEvent clickEvent)
        {
            sph.ToggleSimulationRunning();
            fluidRayMarching.ToggleSimulationRunning();
        }

        private void OnClickResetButton(ClickEvent clickEvent)
        {
            sph.ResetSimulation();
            fluidRayMarching.ResetSimulation();
        }

        private const string InformationLabelText = "Press the [W] [A] [S] [D] keys to start moving the camera.\nPress the [E] and [D] keys for moving up and down.\nPress the [Left Shift] key to speed up.\nPress the [R] key to teleport to the start position.";

        private void OnCursorUnlocked()
        {
            informationLabel.text = InformationLabelText;
        }

        private void OnCursorLocked()
        {
            informationLabel.text = $"Press [ESC] to leave the free camera mode.\n{InformationLabelText}\nMove the mouse to rotate the camera.";
        }

        private void OnClickSaveButton(ClickEvent clickEvent)
        {
            //TODO: Make notifications stack up and not overwrite each other.
            const string errorTextTemplate = "Error writing \"{0}\" file.";
            const string successText = "Files saved successfully at \"{0}\".";

            if
            (
                !ConfigurationFileUtility.TryWrite
                (
                    new ConfigurationColor
                    (
                        color: new SerializableFloat3(colorPickerFluid.SelectedColor.r, colorPickerFluid.SelectedColor.g, colorPickerFluid.SelectedColor.b)
                    )
                )
            )
            {
                notificationManager.ShowNotification(ActionNotificationType.Error, string.Format(errorTextTemplate, nameof(ConfigurationColor)));
            }

            if
            (
                !ConfigurationFileUtility.TryWrite
                (
                    new ConfigurationParticle
                    (
                        viscosity: viscosity.value, 
                        gasConstant: gasConstant.value, 
                        restDensity: restDensity.value, 
                        boundDamping: boundDamping.value
                    )
                )
            )
            {
                notificationManager.ShowNotification(ActionNotificationType.Error, string.Format(errorTextTemplate, nameof(ConfigurationParticle)));
            }

            if
            (
                !ConfigurationFileUtility.TryWrite
                (
                    new ConfigurationSimulation
                    (
                        numberOfParticles: numberOfParticles.value, 
                        containerSize: containerSize.value, 
                        startPosition: startPosition.value,
                        gravityDirection: directionGizmoGravity.ForceDirection,
                        gravityStrength: directionGizmoGravity.Strength,
                        windDirection: directionGizmoWind.ForceDirection,
                        windStrength: directionGizmoWind.Strength
                    )
                )
            )
            {
                notificationManager.ShowNotification(ActionNotificationType.Error, string.Format(errorTextTemplate, nameof(ConfigurationSimulation)));
            }

            var savePath = ConfigurationFileUtility.GetConfigurationPath<ConfigurationSimulation>();
            notificationManager.ShowNotification(ActionNotificationType.Success, string.Format(successText, Path.GetDirectoryName(savePath)));
        }

        private void OnClickLoadButton(ClickEvent clickEvent)
        {
            const string errorTextTemplate = "Problem loading {0} file.";
            const string successText = "Configuration loaded successfully.";

            bool hadError = false;

            if (ConfigurationFileUtility.TryLoad(out ConfigurationColor loadedConfigurationColor))
            {
                SetShaderColorFluid(loadedConfigurationColor);
                SetColorValuesByConfigurationColor(loadedConfigurationColor);
            }
            else
            {
                notificationManager.ShowNotification(ActionNotificationType.Error, string.Format(errorTextTemplate, nameof(ConfigurationColor)));
                hadError = true;
            }

            if (ConfigurationFileUtility.TryLoad(out ConfigurationParticle loadedConfigurationParticle))
            {
                sph.SetShaderValuesByConfigurationParticle(loadedConfigurationParticle);
                SetParticleValuesByConfigurationParticle(loadedConfigurationParticle);
            }
            else
            {
                notificationManager.ShowNotification(ActionNotificationType.Error, string.Format(errorTextTemplate, nameof(ConfigurationParticle)));
                hadError = true;
            }
            
            if(ConfigurationFileUtility.TryLoad(out ConfigurationSimulation loadedConfigurationSimulation))
            {
                sph.SetShaderVector(SPHComputeShaderVariableNames.ContainerSize, (Vector3)loadedConfigurationSimulation.ContainerSize.GetValueOrDefault());
                SetSimulationValuesByConfigurationSimulation(loadedConfigurationSimulation);
            }
            else
            {
                notificationManager.ShowNotification(ActionNotificationType.Error, string.Format(errorTextTemplate, nameof(ConfigurationSimulation)));
                hadError = true;
            }

            if (!hadError)
            {
                notificationManager.ShowNotification(ActionNotificationType.Success, successText);
            }
            UnhighlightPreset();
        }

        private void SetShaderColorFluid(ConfigurationColor loadedConfigurationColor)
        {
            fluidRayMarching.SetShaderVector(FluidRayMarchingShaderVariableNames.FluidColor, (Vector3)loadedConfigurationColor.Color);
        }

        private void RegisterFluidColorCallbacks()
        {
            colorPickerFluid.OnColorChanged += color => 
            {
                fluidRayMarching.SetShaderVector(FluidRayMarchingShaderVariableNames.FluidColor, color);
                sph.SetParticleColor(color);
            };
        }

        private void RegisterParticlePropertiesCallbacks()
        {
            viscosity.RegisterValueChangedCallback(changeEvent =>
            {
                UnhighlightPreset();
                sph.SetShaderFloat(SPHComputeShaderVariableNames.Viscosity, changeEvent.newValue);
            });
            gasConstant.RegisterValueChangedCallback(changeEvent =>
            {
                UnhighlightPreset();
                sph.SetShaderFloat(SPHComputeShaderVariableNames.GasConstant, changeEvent.newValue);
            });
            restDensity.RegisterValueChangedCallback
            (
                changeEvent => 
                {
                    UnhighlightPreset();
                    sph.SetShaderFloat(SPHComputeShaderVariableNames.RestDensity, changeEvent.newValue);
                    sph.RecalculateParticleMass(changeEvent.newValue);
                }
            );
            boundDamping.RegisterValueChangedCallback(changeEvent => sph.SetShaderFloat(SPHComputeShaderVariableNames.BoundDamping, changeEvent.newValue));
        }

        private void RegisterSimulationPropertiesCallback()
        {
            numberOfParticles.RegisterValueChangedCallback(changeEvent =>
            {
                if (IsAnyComponentZeroOrLess(changeEvent.newValue))
                {
                    return;
                }
                if (!isSuppressingNotifications)
                { 
                    notificationManager.ShowNotification(ActionNotificationType.Warning, "You must restart the simulation for changes to take effect.");
                }
                sph.NumberOfParticles = changeEvent.newValue;
            });

            containerSize.RegisterValueChangedCallback(changeEvent =>
            {
                if (IsAnyComponentZeroOrLess(changeEvent.newValue))
                {
                    return;
                }
                sph.SetContainerSize(changeEvent.newValue);
            });

            // TODO delete?
            startPosition.RegisterValueChangedCallback(changeEvent => sph.StartPosition = changeEvent.newValue);
        }

        private void SetInitialValues()
        {
            isSuppressingNotifications = true;
            SetColorValuesByConfigurationColor(Presets.Color[Presets.KeyStart]);
            SetParticleValuesByConfigurationParticle(Presets.Particle[Presets.KeyStart]);
            SetSimulationValuesByConfigurationSimulation(Presets.Simulation[Presets.KeyStart]);
            isSuppressingNotifications = false;
        }

        private void SetColorValuesByConfigurationColor(ConfigurationColor configurationColor)
        {
            colorPickerFluid.SetColor(configurationColor.Color.GetValueOrDefault());
            sph.SetParticleColor(configurationColor.Color.GetValueOrDefault());
        }

        private void SetParticleValuesByConfigurationParticle(ConfigurationParticle configurationParticle)
        {
            viscosity.SetValueWithoutNotify(configurationParticle.Viscosity.GetValueOrDefault());
            gasConstant.SetValueWithoutNotify(configurationParticle.GasConstant.GetValueOrDefault());
            restDensity.SetValueWithoutNotify(configurationParticle.RestDensity.GetValueOrDefault());
            boundDamping.SetValueWithoutNotify(configurationParticle.BoundDamping.GetValueOrDefault());
        }

        private void SetSimulationValuesByConfigurationSimulation(ConfigurationSimulation configurationSimulation)
        {
            SerializableInt3 particlesConfiguration = configurationSimulation.NumberOfParticles.GetValueOrDefault();
            if (numberOfParticles.value == (Vector3)particlesConfiguration)
            {
                isSuppressingNotifications = true;
            }
            numberOfParticles.value = particlesConfiguration;
            isSuppressingNotifications = false;
            
            containerSize.value = configurationSimulation.ContainerSize.GetValueOrDefault();
            startPosition.value = configurationSimulation.StartPosition.GetValueOrDefault();

            directionGizmoGravity.SetGizmoByConfiguration(configurationSimulation);
            directionGizmoWind.SetGizmoByConfiguration(configurationSimulation);

            var particlesCount = numberOfParticles.value.x * numberOfParticles.value.y * numberOfParticles.value.z;
            SetParticlesCount(particlesCount);

        }

        private void SetParticlesCount(int count)
        {
            particlesCount.text = $"Total particles: {count}";
        }
            
        private void ShowActionNotification(ActionNotificationType actionNotificationType, string text)
        {
            actionNotification.RemoveClassesByNameStartingWith("notification-");

            string newClass = $"notification-{actionNotificationType.ToString().ToLower()}";
            actionNotification.AddToClassList(newClass);

            actionNotification.text = text;
        }

        private void OnParticlesSpawned(SerializableInt3 particles, int particlesLength)
        {
            SetParticlesCount(particlesLength);
            if (particlesLength == 0)
            {
                notificationManager.ShowNotification("Please enter at least one particle to spawn.");
            }
            numberOfParticles.SetValueWithoutNotify(particles);
        }

        private void OnContainerSizeUpdated(Vector3 newContainerSize)
        {
            containerSize.SetValueWithoutNotify(newContainerSize);
        }

        private void OnContainerSizeInvalid(Vector3 newContainerSize)
        {
            notificationManager.ShowNotification("Container size is too small for the amount of existing particles!");
            containerSize.SetValueWithoutNotify(newContainerSize);
        }

        private bool IsAnyComponentZeroOrLess(Vector3 vector3)
            => vector3.x <= 0 || vector3.y <= 0 || vector3.z <= 0;

        private VisualElement fluidPresetGroup;
        private Dictionary<string, Button> presetButtons = new Dictionary<string, Button>();
        private bool isSuppressingNotifications;

        private void CreateFluidPresetButtons()
        {
            fluidPresetGroup = new VisualElement();
            fluidPresetGroup.AddToClassList("preset-group");
            presets.Add(fluidPresetGroup);

            List<string> presetKeys = Presets.Particle.Keys.ToList();
            presetKeys.Remove(Presets.KeyStart);

            foreach (var key in presetKeys)
            {
                var btn = new Button(() => OnPresetButtonClicked(key))
                {
                    text = key
                };

                btn.AddToClassList("preset-button");

                presetButtons[key] = btn;
                fluidPresetGroup.Add(btn);
            }

            HighlightPreset(currentFluidPresetKey);
        }

        private void OnPresetButtonClicked(string key)
        {
            currentFluidPresetKey = key;

            ConfigurationColor color = Presets.Color[key];
            SetColorValuesByConfigurationColor(color);
            SetShaderColorFluid(color);

            ConfigurationParticle newPreset = Presets.Particle[key];
            sph.SetShaderValuesByConfigurationParticle(newPreset);
            SetParticleValuesByConfigurationParticle(newPreset);
            
            HighlightPreset(key);
        }

        private void HighlightPreset(string key)
        {
            foreach (var kvp in presetButtons)
            {
                kvp.Value.RemoveFromClassList("preset-button-selected");

                if (key != null && kvp.Key == key)
                {
                    kvp.Value.AddToClassList("preset-button-selected");
                }
            }
        }

        private void UnhighlightPreset()
        {
            foreach (var kvp in presetButtons)
            {
                kvp.Value.RemoveFromClassList("preset-button-selected");
            }
        }

        private void ClearPresetHighlight()
        {
            HighlightPreset(null);
        }

    }
}