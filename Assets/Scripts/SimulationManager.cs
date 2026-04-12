/*=========================================================================

   Library: iMSTK-Unity

   Copyright (c) Kitware, Inc. 

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

      http://www.apache.org/licenses/LICENSE-2.0.txt

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.

=========================================================================*/

using System.Collections.Generic;
using System.Threading;
using System.Linq;
using UnityEngine;
using Unity.Profiling;
using System;

namespace ImstkUnity
{
    [System.Serializable]
    public class PbdModelConfiguration
    {
        public Vector3 gravity = new Vector3(0.0f, -9.81f, 0.0f);
        public int iterations = 4;
        public bool useRealtime = true;
        public double dt = 0.01;

        public double linearDampingCoeff = 0.01;
        public double angularDampingCoeff = 0.01;

        public bool doPartitioning = false;

        public bool showStats = true;

        public static PbdModelConfiguration Default()
        {
            PbdModelConfiguration result = new PbdModelConfiguration();
            var defaultConfig = new Imstk.PbdModelConfig();
            Imstk.Vec3d gravity = defaultConfig.m_gravity;
            result.gravity = gravity.ToUnityVec();
            result.iterations = (int)defaultConfig.m_iterations;
            result.useRealtime = true;
            result.dt = defaultConfig.m_dt;
            result.linearDampingCoeff = defaultConfig.m_linearDampingCoeff;
            result.angularDampingCoeff = defaultConfig.m_angularDampingCoeff;
            result.doPartitioning = defaultConfig.m_doPartitioning;

            return result;
        }

        public PbdModelConfiguration DeepCopy()
        {
            PbdModelConfiguration result = new PbdModelConfiguration();
            result.gravity = gravity;
            result.iterations = iterations;
            result.useRealtime = useRealtime;
            result.dt = dt;
            result.linearDampingCoeff = linearDampingCoeff;
            result.angularDampingCoeff = angularDampingCoeff;
            result.doPartitioning = doPartitioning;

            return result;
        }

    }

    /// <summary>
    /// Class to be used as a singleton, it is the main interface to all iMSTK functionality
    /// </summary>
    /// Calls ImstkInit and ImstkStart on all ImstkBehaviors, behaviours are responsible for
    /// adding themselves to he current scene. Runs a thread that calls the imstk physics loop
    /// on Update() will wait for the thread to pause and in LateUpdate() it restarts the thread
    [AddComponentMenu("iMSTK/SimulationManager")]
    [DefaultExecutionOrder(-99)]
    public class SimulationManager : Singleton<SimulationManager>
    {
        public static Imstk.SceneManager sceneManager = null;

        static readonly ProfilerMarker s_AdvancePerfMarker = new ProfilerMarker(ProfilerCategory.Physics, "Imstk.Advance");

        // Timestep passed into the simulation
        public float fixedTimestep = 0.01f;

        public bool writeTaskGraph = false;

        private Imstk.CacheOutput output;

        public PbdModelConfiguration pbdModelConfiguration = new PbdModelConfiguration();
        public static Imstk.RigidBodyModel2 rigidBodyModel = null;
        public static Imstk.PbdModel pbdModel = null;

        [SerializeField]
        public CollisionInteractions collisions = new CollisionInteractions();

        private AccumulatingBuffer _frameTimes = new AccumulatingBuffer(100); // Unity Seconds
        private TimingBuffer _physicsTimes = new TimingBuffer(100); // Milliseconds

        private Thread _simulationThread;

        // See https://makolyte.com/csharp-thread-safe-primitive-properties-using-lock-vs-interlocked/
        private long _shouldRun = 0;
        private bool ShouldRun
        {
            get
            {
                /* Interlocked.Read() is only available for int64,
                 * so we have to represent the bool as a long with 0's and 1's
                 */
                return Interlocked.Read(ref _shouldRun) == 1;
            }
            set
            {
                Interlocked.Exchange(ref _shouldRun, Convert.ToInt64(value));
            }
        }
        
        // Used to synchronize time between the imstk thread and unity
        private float _targetTime = 0.03f;


        Imstk.Scene _currentScene = null;
        public static bool HasFrame = false;

        public AccumulatingBuffer FrameTimes
        {
            get { return _frameTimes; }
        }
        public TimingBuffer PhysicsTimes
        {
            get { return _physicsTimes; }
        }

        public float EngineTime
        {
            get { 
                if (_currentScene == null)
                    return 0f;
                return (float)(_currentScene.getFrameTime() * 1000.0); 
            }
        }

        /// <summary>
        /// Returns all components in the scene of a given type.
        /// This allows us to collect the iMSTK components and insert them into iMSKT no
        /// matter where they are located. Components that are not "enable" will be disregarded
        /// </summary>
        /// <typeparam name="T">Class of the Component that you are looking for, needs to be a Monobehavior</typeparam>
        /// <returns>List of all _active_ components in the scene of type T</returns>
        public static List<T> GetAllComponents<T>() where T : MonoBehaviour
        {
            List<T> behaviours = new List<T>();
            List<GameObject> objects = FindObjectsOfType<GameObject>().ToList();
            foreach (GameObject obj in objects)
            {
                foreach (var behaviour in obj.GetComponents<T>())
                {
                    if (behaviour.isActiveAndEnabled)
                    {
                        behaviours.Add(behaviour);
                    }
                }
            }
            return behaviours;
        }

        private void Awake()
        {
            // Get the settings
            ImstkSettings settings = ImstkSettings.Instance();
            //if (settings.useOptimalNumberOfThreads)
            //    settings.numThreads = 0;

            Imstk.Logger.startLogger();
            output = Imstk.Logger.instance().getCacheOutput();

            // Create the simulation manager
            sceneManager = new Imstk.SceneManager();
            sceneManager.setActiveScene(new Imstk.Scene("DefaultScene"));
            sceneManager.getActiveScene().getConfig().writeTaskGraph = writeTaskGraph;

            // Create A single PbdModel to share for all pbd bodies used in the scene 
            CreatePbdModel();
        }

        private void Start()
        {
            // It seems that InitManager needs to come AFTER the call that creates the
            // device inside of the OpenHapticsDevice
            IntializeImstkStructures();

            sceneManager.init();

            // Start order
            {
                List<ImstkBehaviour> behaviours = GetAllComponents<ImstkBehaviour>();
                foreach (ImstkBehaviour behaviour in behaviours)
                {
                    behaviour.ImstkStart();
                }
            }

#if IMSTK_USE_OPENHAPTICS
            OpenHapticsDevice.InitManager();
#endif

#if IMSTK_USE_VRPN
            if (VrpnDeviceManager.Instance != null) VrpnDeviceManager.Instance.StartManager();
#endif

            // #refactor should follow the same pattern as all
            // i.e. Get all Managers in the scene and start them
            // use same pattern for both managers

#if IMSTK_USE_OPENHAPTICS
            OpenHapticsDevice.StartManager();
#endif
            _currentScene = sceneManager.getActiveScene();



        }

        private void RunImstk()
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

            ShouldRun = true;
            stopwatch.Restart();
            double realTimeDifference = 0;
            while (ShouldRun)
            {
                // _targetTime will be set in the SimulationManagers Update() 
                // function, it's the previous frames Time.deltaTime, we're trying
                // we'll only calculate enough simulation time to match the _targetTime
                if (_targetTime > 0)
                {
                    _physicsTimes.Begin();
                    Monitor.Enter(_simulationThread);

                    // Investigate iMSTK "RealTime"
                    if (pbdModelConfiguration.useRealtime)
                    {
                        pbdModel.getConfig().m_dt = fixedTimestep;
                    }

                    s_AdvancePerfMarker.Begin();

                    if (_currentScene != null)
                    {
                        _currentScene.advance(fixedTimestep);
                    }

                    s_AdvancePerfMarker.End();
                    Monitor.PulseAll(_simulationThread);
                    Monitor.Exit(_simulationThread);

                    _targetTime -= fixedTimestep;

                    // Check the difference between simulation time (dt)
                    // and realtime (elapsed) accumulate and pause 
                    // the thread if we're ahead of time
                    // nothing is done if imstk is running behind 
                    realTimeDifference += fixedTimestep - stopwatch.Elapsed.TotalSeconds;
                    if (realTimeDifference > 0.005)
                    {
                        int sleepTime = (int)(realTimeDifference * 1000);
                        realTimeDifference -= sleepTime / 1000.0;
                    }
                    stopwatch.Restart();
                    _physicsTimes.End();
                }
                else
                {
                    // Busy Wait ...
                    Thread.Sleep(1);
                }
            }
        }


        private void Update()
        {
            // TODO figure out a better way to start 
            if (_simulationThread == null)
            {
                _simulationThread = new Thread(new ThreadStart(RunImstk));
                _simulationThread.Start();
            }

            Monitor.Enter(_simulationThread);
            HasFrame = true;
            // Probably needs to be a better delta time (e.g. the sim time from the last simulation frames)
            if (_currentScene != null)
            {
                _currentScene.updateVisuals(Time.deltaTime);
            }

            _targetTime += Time.deltaTime;
            _frameTimes.Push(Time.deltaTime);
            LogToUnity();
        }

        private void LateUpdate()
        {
            // Free lock again ..
            Monitor.PulseAll(_simulationThread);
            Monitor.Exit(_simulationThread);
        }

        private void OnDestroy()
        {
            // Shut down thread
            ShouldRun = false;
            _simulationThread.Join();

            // Destroy order
            {
                List<ImstkBehaviour> behaviours = GetAllComponents<ImstkBehaviour>();
                foreach (ImstkBehaviour behaviour in behaviours)
                {
                    behaviour.ImstkDestroy();
                }
            }

            sceneManager.uninit();

#if IMSTK_USE_OPENHAPTICS
            OpenHapticsDevice.StopManager(); // Stops in async
#endif
#if IMSTK_USE_VRPN
            if (VrpnDeviceManager.Instance != null) VrpnDeviceManager.Instance.StopManager();
#endif
            _currentScene = null;
            // These are static so we need to make sure to set them to null on quit
            sceneManager = null;
        }

        private void IntializeImstkStructures()
        {
            // SimulationManager initializes objects in a particular order
            List<ImstkBehaviour> behaviours = GetAllComponents<ImstkBehaviour>();
            foreach (ImstkBehaviour behaviour in behaviours)
            {
                behaviour.ImstkInit();
            }

            foreach (ImstkBehaviour behaviour in behaviours)
            {
                behaviour.ImstkComponentInit();
            }

            // We need to ensure all objects are created before interactions and controllers
            // are setup using them
            CreateCollisionInteractions();

            List<ImstkInteractionBehaviour> interactions = GetAllComponents<ImstkInteractionBehaviour>();
            foreach (ImstkInteractionBehaviour behaviour in interactions)
            {
                Imstk.SceneObject interaction = behaviour.GetImstkInteraction();
                if (interaction != null)
                {
                    sceneManager.getActiveScene().addInteraction(interaction);
                }
            }

            List<ImstkControllerBehaviour> controllers = GetAllComponents<ImstkControllerBehaviour>();
            foreach (ImstkControllerBehaviour behaviour in controllers)
            {
                // Currently only support tracking device controls
                Imstk.TrackingDeviceControl control =
                    behaviour.GetController() as Imstk.TrackingDeviceControl;
                if (control != null)
                {
                    sceneManager.getActiveScene().addControl(control);
                }
            }
        }

        private void LogToUnity()
        {
            string m;
            while (output.hasMessages())
            {
                m = output.popLastMessage();
                if (m.Contains("FATAL"))
                {
                    Debug.LogError(m);
                }
                else if (m.Contains("WARNING"))
                {
                    Debug.LogWarning(m);
                } else
                {
                    Debug.Log(m);
                }
           }
        }

        private void CreatePbdModel()
        {
            pbdModel = new Imstk.PbdModel();
            Imstk.PbdModelConfig config = new Imstk.PbdModelConfig();

            config.m_dt = pbdModelConfiguration.dt;
            config.m_gravity = pbdModelConfiguration.gravity.ToImstkVec();
            config.m_iterations = (uint)pbdModelConfiguration.iterations;
            config.m_linearDampingCoeff = pbdModelConfiguration.linearDampingCoeff;
            config.m_angularDampingCoeff = pbdModelConfiguration.angularDampingCoeff;
            config.m_doPartitioning = pbdModelConfiguration.doPartitioning;
            pbdModel.configure(config);
            pbdModel.setTimeStepSizeType((pbdModelConfiguration.useRealtime) ? Imstk.TimeSteppingType.RealTime : Imstk.TimeSteppingType.Fixed);
            pbdModel.setVelocityThreshold(100000000);
        }

        private void CreateCollisionInteractions()
        {
            foreach (var item in collisions._collisions)
            {
                if (item.model1 == null || item.model2 == null ||
                    !item.model1.isActiveAndEnabled || !item.model2.isActiveAndEnabled)
                {
                    if (item.model1 != null && item.model2 != null)
                    {
                        Debug.Log(
                            $"Collision: {item.model1.name}/{item.model2.name} is not active or enabled, skipping");
                    }
                    continue;
                }

                var pbd = Imstk.Utils.CastTo<Imstk.PbdObject>(item.model1.GetDynamicObject());
                Debug.Assert(pbd != null);
                Debug.Assert(item.model2.GetDynamicObject() != null);
                // At the moment all colliding objects are PBD 
                // PbdObjectCollision expects first parameter to be PBD, the second parameter
                // may be any type of CollidingObj
                // TODO Add collision detectiontype

                var cdType = (item.collisionTypeName == "Auto") ? "" : item.collisionTypeName;  
                Imstk.PbdObjectCollision collision =
                    new Imstk.PbdObjectCollision(
                        pbd,
                        item.model2.GetDynamicObject(), cdType);

                Debug.Log($"Collision: {item.model1.name}/{item.model2.name} with {cdType} created");
                
                // Just configure ALL the parameters
                collision.setDeformableStiffnessA(item.deformableStiffness1);
                collision.setDeformableStiffnessB(item.deformableStiffness2);
                collision.setRigidBodyCompliance(item.rigidBodyCompliance);
                collision.setFriction(item.friction);
                collision.setRestitution(item.restitution);
                collision.setUseCorrectVelocity(false);
                sceneManager.getActiveScene().addInteraction(collision);

                item.model1.collisionInteractions.Add(collision);
                item.model2.collisionInteractions.Add(collision);

                item.model1.AddCollisionWith(item.model2, collision);
                item.model2.AddCollisionWith(item.model1, collision);
            }
        }
    }
}