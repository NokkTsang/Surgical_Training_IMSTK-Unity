
# Mixed Reality (MR) Surgical Training App for Meta Quest

This is a Unity-based mixed reality application for surgical training, powered by the [Interactive Medical Simulation Toolkit (iMSTK)](https://www.imstk.org/). The project delivers real-time deformable tissue simulation, needle–tissue interaction, and suturing mechanics specifically designed for Meta Quest devices in medical training scenarios.

## 🎯 Project Overview

### Key Features

**Suturing Simulation**
The suturing module demonstrates full needle puncture, thread insertion, and stitching using iMSTK 7.0's Position-Based Dynamics (PBD) engine. The needle interacts with deformable tissue in real time — puncturing, threading, and closing wounds with physically-based constraints.

**Deformable Tissue Interaction**
Integrates iMSTK's PBD solver to provide real-time deformable organ models with tetrahedral mesh physics, surface collision detection, and instrument-to-tissue interactions for surgical training scenarios.

## 📋 Requirements

### Development Environment
- **Unity Hub**: 3.12.1+
- **Unity Editor**: 2022.3 LTS (tested with 2022.3.62f1)
- **Visual Studio**: 2019 or 2022 Build Tools (for building iMSTK C++ from source)

### Packages
- AR Foundation (5.2.2+)
- XR Interaction Toolkit (2.6.5+)
- XR Plugin Management (4.5.4+)
- Oculus XR Plugin (4.5.4+)
- iMSTK 7.0 (included as prebuilt DLLs in `Assets/Plugins/x86_64/`)

### Platform Support
- Meta Quest 2
- Meta Quest 3
- Meta Quest Pro

## 🛠️ Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/NokkTsang/Surgical_Training_IMSTK-Unity.git --recurse-submodules
   ```
2. Open the project in Unity 2022.3 LTS via Unity Hub.
3. The prebuilt iMSTK DLLs are included in `Assets/Plugins/x86_64/` — no C++ build required for running the scenes.
4. Open the suturing scene at `Assets/Scenes/Devices/Suturing.unity`.

### Building iMSTK from Source (Optional)

If you need to modify the C++ iMSTK source:

1. Ensure Visual Studio 2019 Build Tools and CMake are installed.
2. Run `ImstkSource~/InstallImstk.bat` or build manually:
   ```bash
   cd ImstkSource~/build
   cmake .. -DiMSTK_BUILD_FOR_UNITY=ON
   cmake --build . --config Release
   ```
3. Copy the rebuilt `iMSTKCWrapper.dll` from `ImstkSource~/build/install/bin/` to `Assets/Plugins/x86_64/`.

## 📄 License

This project is licensed under the [Apache 2.0 License](http://www.apache.org/licenses/LICENSE-2.0.txt) — see the [LICENSE](LICENSE.txt) file for details.
