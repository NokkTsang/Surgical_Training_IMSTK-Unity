Usage
=====

For minimal usage of iMSTK-Unity, two things must be added to a Unity Scene.

   - A ``GameObject`` with a ``SimulationManager`` attached to it
   - A ``GameObject`` with either a ``Deformable`` or a ``Rigid``.

Commonly a PhysicsGeometry is also needed on the Model ``GameObject``.

This document will denote some of the basic classes available in iMSTK-Unity. For more information refer to the the source code documentation and the iMSTK documentation.

Component Structure
==============================

While the iMSTK C# wrapper supports almost all iMSTK classes. There is a subset that is made available as Unity components. These can be assembled in the editor to create simulations using iMSTK inside of Unity. The following section describes the roles and responsibilities of the available iMSTK-Unity classes.

While most components can be enabled and disabled in the Editor this will only be effective during the editing process. Disabled components will not be used for simulation. _BUT_ enabling or disabling a component during runtime will not affect the simulation and may cause issues. We also have made efforts to check for disabled components in other components that depend on them. If you find any combinations that do not work correctly please let us know.

Infrastructure
------------------------------

``SimulationManager``
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
This is a component responsible for controlling the simulation. There may only exist one. It also controls the construction, initialization, and destruction of ``iMSTKBehaviour`` to ensure execution ordering: 

    - Simulation Manager created
    - iMSTK objects created and internally initialized
    - iMSTK objects externally initialized
    - ``SimulationManager`` Start
    - Updates 
    - iMSTK objects cleaned up 
    - ``SimulationManager`` cleaned up.

This component is required to be in the scene for simulations to run. It is created before any other iMSTK components on any ``GameObject``. It implements the start, stop, pause, and other global scene related tasks.


``iMSTKBehaviour``
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
An extension of MonoBehaviour to provide different callbacks for special construction, initialization, and destruction ordering. This is the base class for most iMSTK components. If you are creating a new component that needs to be initialized by the ``SimulationManager`` it should inherit from this class. 


Models
------------------------------
The following classes are the building blocks of any simulation scene, these are the things that interact with each other and the world.

``Deformable``
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
Use this to represent deformable objects. Position Based Dynamics (PBD), is used to model the deformation. This model supports Lines (1D), Surface Meshes (2D) and, Tetrahedral Meshes (3D) dynamical models see the `iMSTK Documentation <https://imstk.gitlab.io/Dynamical_Models/PbdModel.html>`__ for more information on constraints and models. Visual, physics and collision geometry can be assigned separately. If you do, a separate map will be necessary to update the various meshes. 

The physics geometry determines the type of constraint that can be used, an invalid constraint may cause problems. 

.. list-table:: Valid Constraint Combinations
   :header-rows: 1

   * - Physics Geometry Type
     - Valid Constraints
   * - Line Mesh (Threads)
     - Distance Stiffness, Bend Stiffness
   * - Surface Mesh (Membranes, Bags)
     - Distance Stiffness, Dihedral Stiffness, Area Stiffness
   * - Volumentric Mesh (Tissue)
     - Distance Stiffness, Volume Stiffness, Fem (all models)

``Rigid``
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
Use this to represent movable rigid object use this to represent movable rigid objects like forceps or scalpels.
Physics and collision geometry can be assigned separately.
Implements a rigid body using position based dynamics from imstk.
Please note there are two ways rigids will be used in the simulation, one 
is as free rigid bodies like a needle or staples. The other is as 
tools that are driven via a controller through a device.
Currently free rigids cannot be transformed through a unity parent transform.

``StaticModel``
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
Use this to represent un-moveable rigid objects like the ground plane or other obstacles.

Model Support
------------------------------
When creating a model you will need to assign a geometry to it. And possibly a geometry map as well.

``GeometryFilter``
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
Similar to a MeshFilter in Unity. It provides an input and output geometry. It may take in any iMSTK geometry, as well as a Unity Mesh (one can also drag/drop a MeshFilter to it). These are instances of geometries used in all of iMSTK unity scripts. Instances of this class fit into the ``Visual Geometry``, ``Physics Geometry``, and ``Collision Geometry`` slots on the model  components.

``GeometryMap``
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
Allows the use of separate meshes for the deformable, visual and collision representation. Will move the vertices of the target mesh according to matching points on the source mesh The points do not have to completely coincide. In almost all cases you will need to map FROM the physics mesh TO the visual mesh, and FROM the physics mesh TO the collision mesh.

Supporting Classes
------------------------------
The following classes are used to your simulation development and provide additional functionality. This is not an exhaustive list, please check the source code for more information.

``ConstrainDeformables``
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
This will set up a set of distance constraints between two deformable objects. The constraints will be limited to the area encompassed by the assigned mesh,. constraints will be generated for _all_ pairs of points whose distance is smaller than or equal the cutoff distance. The length of the constraint will be set to the original distance * restLength. Use this if you want to attach a deformable to another deformable. E.g. a vessel to another organ.

``ConstrainInSpace``
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
This will set up a set of distance constraints between the points of deformable that are found inside the constrained area and virtual points, effectively attaching the deformable to those points in space. The constraints will be limited to the area encompassed by the assigned mesh constraints will be generated for _all_ points. The length of the constraint will be set to  ``restLength``. Use this if you want to attach a deformable to a point in space. E.g. Suspend an organ in the body cavity.

``ConnectiveTissue``
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
This component represents connective tissue as a multitude of strands between opposing surfaces. Given two opposing geometries strands will be generated with configurable parameters. The generated object is physical and can be interacted with. The connective tissue will consist of multiple "strands" each going from one of the reference objects to the other. Each strand will be made up of a given number of segments. The amount of strands is roughly the number of faces on one bounding object * ``segmentsPerFace``. Note that increasing the density and/or the number of segments per strand will also increase the computational load to simulate this object.

``RigidController``
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
Object used between a device handled by the user and a ``Rigid``. It utilizes a mass spring system to correct for latency in the system. It corrects for problems with haptics in simulation systems. By manipulating the spring parameters the haptic response can be tuned to the behavior of the computer and the simulated system.

Collisions
------------------------------
While you can set up a collision using the ``CollisionInteraction`` class it is easier to use the ``Collisions`` panel that is situated in both of the ``Deformable`` and ``Rigid`` components. This panel will allow you to set up collisions between the object and the world, as well as other objects. ``CollisionInteraction`` class is used to set up collisions between two objects.

``CollisionInteraction``
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
Use this behavior to set up collisions between two objects, in general this behavior can detect what the type of the two objects is that are interacting (mode `Auto`). But you can also select the algorithm that should be used.

Grasping
------------------------------
Grasping is handled via the ``Grasping`` component. But in almost all cases it will be easier to utilize the ``GraspingManager`` as it will handle the creation and destruction of the ``Grasping`` component for you. If you have tools that you want to manipulate you can also investigate the ``GraspingController`` it can play hand in hand with the above components but also deals with animating tool jaws for example.


Importers
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
iMSTK-Unity provides a custom Unity importer to import geometry using iMSTK. This can import point, line, surface, tetrahedral, & hexahedral meshes (vtk, vtu, stl, ply, veg, ...). If the mesh imported is a point, line, or surface mesh then it will be imported as a Unity Mesh object. Anything else not supported by Unity, is loaded as an iMSTK-Unity Geometry Object. When a volumetric mesh (such as a tetrahedral mesh) is imported the accompanying surface is extracted and provided as an additional asset.

Editor Scripts
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
iMSTK-Unity provides extensions to the Unity editor. These extensions include:

   - Custom inspectors for the models and geometry components.
   - A global settings menu.
   - Menu Items for quick creation of GameObject with iMSTK items already setup.
   - Editors/windows for various operations

Devices
------------------------------

``OpenHapticsDevice``
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
This device is only available with a custom build of iMSTK. It enables the use of the `3DSystems <https://www.3dsystems.com/haptics>`_ haptic device family.

``VrpnDevice``
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
This device is only available with a custom build if iMSTK. It enables interactions with devices run by a `VRPN <https://github.com/vrpn/vrpn>`_ server.

Other
------------------------------

``SimulationStats``
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
This component can be used to display timing information on the game screen, this is aside the data that is pushed to the profiler. The data shown is the ``Update()`` rate, the avg. time used to run 1 physics update `simulationManager->advance()` and information about mesh updates.

Classes removed in this version
-------------------------------
- ``RbdModel`` has been removed, use ``Rigid`` instead.
- ``PbdModel`` has been removed, use ``Deformable`` instead.
- ``PbdRigidGraspingInteraction`` has been removed, use ``Grasping`` instead.






