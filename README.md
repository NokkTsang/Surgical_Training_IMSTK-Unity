
## SUNSET NOTICE

Support and development of Interactive Medical Simulation Toolkit (iMSTK) has been discontinued as of May-02-2025. This project is no longer under active development or support.
## About

#### Overview
[iMSTK](https://www.imstk.org/) is a C++ based free & open-source toolkit that aids rapid prototyping of real-time multi-modal surgical simulation scenarios. [Unity](https://unity.com/) is a multi-platform game engine designed to create 2D and 3D games. You will have received this plugin either via the Unity Asset Store or by cloning the git repository.

While the asset contains all the binaries needed to run the plugin the code in the repository only contains the C# scripts, demo scenarios and resources needed to build imstk Unity package. To run it you will need to build iMSTK. The repository contains some folders not included in the Asset, any folder with `~` as the last character will not be visible inside of Unity.

#### License
[Apache 2.0](http://www.apache.org/licenses/LICENSE-2.0.txt)

## Resources

### Documentation

User documentation: [https://imstk-unity.readthedocs.io/en/latest/](https://imstk-unity.readthedocs.io/en/latest/)

API documentation: https://imstk-unity.gitlab.io/documentation/

### Issue-tracker

https://gitlab.kitware.com/iMSTK/imstk-unity/issues

### Support 

Discourse: https://discourse.kitware.com/c/imstk/

## Building iMSTK-Unity

#### Prerequisites
* Git
* Unity 2021.3
* Visual Studio 2022 (Not tested with older versions)

To checkout use `git clone https://gitlab.kitware.com/iMSTK/imstk-unity.git --recurse-submodules` this will checkout all of iMSTK-Unity and should also check out the correct version of iMSTK into the `iMSTKSource~` directory, where you will be able to build the binaries.

## Build Instructions

See [user documentation](https://imstk-unity.readthedocs.io/en/latest/documentation.html#setup-for-development) for build instructions. 

## Contact 

Contact Kitware at https://www.kitware.com/contact-us/

## Known Issues

- Starting the scene with a OpenHaptics device in the scene but no device plugged in will crash Unity
- When using iMSTK with an OpenHaptics ethernet device the simulation may crash Unity
- Creating deformables with different sets of contraints may not work
