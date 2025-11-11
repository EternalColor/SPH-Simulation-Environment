# SPH-Simulation-Environment

## Description

The main part of this work was the evaluation and implementation of a user interface that allows the various parameters of the simulation, such as viscosity, density, particle count, external forces (gravity and wind), obstacles, or color selection, to be controlled in real time. This allows users to observe the behavior of liquids in a wide variety of scenarios. In addition, presets for fluid types such as water, lava, honey, alcohol, and oil were created so that users can quickly switch between simulation scenarios.

Reference simulation environments were analyzed in the evaluation of the UI. Based on this analysis, three different UI mockups were created. User surveys were conducted to select a template for my user interface from these three mockups. The user interface was implemented using the Unity UI Toolkit. Specialized UI elements were created for the external forces to make them as easy to configure as possible.

The SPH-Algorithm was optimized with a spatial grid. This allowed the time complexity to be optimized from O(n²) to O(n). Additional functionalities such as saving and loading the simulation configuration as JSON files, validation of user input, and free camera movement are also available. Unit and integration tests for the core functionalities were created to confirm the physically correct behavior of the simulation.

## Most Important Features

### Change fluid parameters
UI Element | Particles
--|--
<video src="https://github.com/user-attachments/assets/1f9fa20d-c7af-47cd-bb24-955c2fe8cfb4" autoplay loop muted playsinline></video>|<video src="https://github.com/user-attachments/assets/13218442-84d3-402d-a348-1b24eb7413c5" autoplay loop muted playsinline></video>

### Change external forces

UI Element | Particles
--|--
<video src="https://github.com/user-attachments/assets/d9203042-016b-4490-8eb8-1813716d998b" autoplay loop muted playsinline></video>|<video src="https://github.com/user-attachments/assets/106526f6-d62d-455b-a5ac-98a2ef2cc002" autoplay loop muted playsinline></video>

### Choose from different fluid presets

UI Element | Particles
--|--
<video src="https://github.com/user-attachments/assets/4c7439a8-c00b-4974-b10e-68fd42153b36" autoplay loop muted playsinline></video>|<video src="https://github.com/user-attachments/assets/62cdc44c-b938-4b1f-90ed-8780d48bcb59" autoplay loop muted playsinline></video>

### Add obstacles to the simulation

UI Element | Particles
--|--
<video src="https://github.com/user-attachments/assets/9ead80da-e47f-4871-a690-8d037d62af26" autoplay loop muted playsinline></video>|<video src="https://github.com/user-attachments/assets/7382509f-8c01-49c3-942b-64b34aad6957" autoplay loop muted playsinline></video>

### Control the simulation

UI Element | Particles
--|--
<video src="https://github.com/user-attachments/assets/f381ae61-9af1-46b1-9396-fbc5ce3a836b" autoplay loop muted playsinline></video>|<video src="https://github.com/user-attachments/assets/4c9ea617-df47-4c6f-8933-a985617f065f" autoplay loop muted playsinline></video>

### Change fluid colors

UI Element | Particles
--|--
<video src="https://github.com/user-attachments/assets/f908e1e0-7d82-4c80-a2f9-d8a73f5a5c24" autoplay loop muted playsinline></video>|<video src="https://github.com/user-attachments/assets/74176662-8a7c-4e36-9d6b-f8132225ad99" autoplay loop muted playsinline></video>

## Screenshots

<img width="1408" height="758" alt="Simulation of wave patterns in water inside a narrow container" src="https://github.com/user-attachments/assets/1d25af96-3bf7-45c1-9ac2-642f8cb6601a" />
<img width="1404" height="790" alt="Simulation of water reacting with wind and obstacles" src="https://github.com/user-attachments/assets/6b53968f-886b-4b6a-86f5-0db6a84a03af" />
