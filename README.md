# 🧭 Spherical Karlsruhe Metric Visualization in Unity
An interactive Unity-based visualization of Voronoi diagrams on a sphere using the Karlsruhe metric, geodesic distance, and hybrid comparisons. This project was developed as part of an advanced software practical at university, combining shader programming, spherical geometry, and projection techniques.

## 📌 Features
- 🌐 Spherical Voronoi Diagrams: Rendered directly on a 3D sphere using a custom fragment shader.

- 🧮 Metric Switching:
  - Geodesic (Euclidean) distance on the sphere.
  - Spherical Karlsruhe Metric extension.
  - Overlap Mode: Highlights regions where both metrics agree.

- 🌀 Grow Animation: Cells expand from reference points according to the selected metric.
- 🖱️ Interactive Reference Points:
  - Randomly generated based on user-defined count.
  - Fully draggable on both the sphere and projections.

- 🧭 Projection Views:
  - Mercator Projection
  - Azimuthal Projection (centered at North and South poles)
  - All projections are synchronized with the main sphere.

- 🔄 Sphere Rotation: Freely rotatable for full-sphere exploration.

## 🎓 Motivation
This project explores the extension of the Karlsruhe metric to spherical geometry and its implications for spatial partitioning. It serves as a visual and interactive tool for comparing geodesic and Karlsruhe-based Voronoi cells, especially in contexts where spherical surfaces are relevant (e.g., planetary mapping, global networks).

## 🛠️ Technologies Used
- Unity Engine (2022.x or later recommended)
- Unity Shader Language (HLSL/ShaderLab)
- C# for input handling and synchronization
- GPU fragment shaders for real-time Voronoi computation

## 📷 Screenshots
### Sphere divided by Karlsruhe Metric Voronoi diagram
<img width="1168" height="1133" alt="Karlsruhe-nearest-many" src="https://github.com/user-attachments/assets/1232cd3a-305f-4f31-b9fb-985ae189242f" />
### Sphere divided by Geodesic Metric Voronoi diagram
<img width="1140" height="1158" alt="Geodesic-nearest-many" src="https://github.com/user-attachments/assets/ba689b0a-8844-4bd1-b8d0-d35833d9c0e3" />
### Mercator sphere prjection divided by Karlsruhe Metric Voronoi diagram
<img width="1281" height="1223" alt="KarlsruheMetric-topDown" src="https://github.com/user-attachments/assets/19a12c23-4d84-44c7-82d9-c86f5059e618" />
### Azimuthal sphere prjection divided by Geodesic Metric Voronoi diagram
<img width="1577" height="886" alt="Geodesic-furthest-many-mercator" src="https://github.com/user-attachments/assets/14319b00-0019-4f43-9f5a-d7a298fbbfe8" />


## 🚀 Getting Started
1. Download the realease and extract it.
2. Run the .exe file and explore the visualization.

1. Alternatively, clone the repository:
```bash
https://github.com/SebastianZins/Spherical-Karlsruhe-Metric
```
2. Open the project in Unity.
3. Press Play to explore the visualization.

Use the UI to:
- Switch metrics
- Rotate the sphere
- Adjust number of reference points
- Toggle projection views

## 🧪 Known Issues
- Shader precision may vary across platforms—test on desktop for best results.
- Dragging points in the azimuthal projection may break the program if the points are moved to fast.

## 📚 Academic Context
This project was developed as part of a university practicum focused on extending the Karlsruhe metric to spherical spaces. It includes:
- Comparative analysis of geodesic vs. Karlsruhe Voronoi cells
- Projection-based visualization for intuitive understanding
- Interactive tools for exploring metric behavior

## 📄 License
MIT License. See LICENSE.md for details.
