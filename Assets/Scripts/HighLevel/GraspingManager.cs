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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ImstkUnity
{
    /// <summary>
    /// Set ups multiple grasping components, handles grasping and upgrasping
    /// this set of components. This alleviates the burden to the user to set 
    /// up multiple grasping components. Plays together with the Grasping Controller
    /// that handles the User interaction side of making a tool that can grasp, but
    /// can also be used on its own
    /// </summary>
    public class GraspingManager : MonoBehaviour
    {
        [Serializable]
        public struct GraspingData
        {
            public bool enabled;
            public bool disableCollisionOnGrasp;
            public DynamicalModel target;
            public Grasping.GraspType type;
        }

        public Rigid rigid;
        public bool useUnityTransform = false;
        public bool requireTouch = false;
        public List<GeometryFilter> graspingGeometries;
        public List<GraspingData> graspingData = new List<GraspingData>();

        public List<Grasping> graspings = new List<Grasping>();
        Coroutine _coroutine;

        public double deformableGraspingStiffness = 0.4;
        public double rigidGraspingStiffness = 1 / 0.0001;

        void Awake()
        {
            // These will be created _before_ the simulation manager searches for 
            // its component and therefor will participate in the norm imstk-unity
            // discovery
            if (!isActiveAndEnabled) { return; }
            foreach (var item in graspingData.Where(a => a.enabled && a.target != null))
            {
                if (item.target.gameObject.activeInHierarchy)
                {
                    foreach (var geom in graspingGeometries)
                    {
                        var grasping = gameObject.AddComponent<Grasping>();
                        grasping.rigidModel = rigid;
                        grasping.graspingGeometry = geom;
                        grasping.graspedObject = item.target;
                        grasping.graspType = item.type;
                        grasping.deformableGraspingStiffness = deformableGraspingStiffness;
                        grasping.rigidGraspingStiffness = rigidGraspingStiffness;
                        grasping.useUnityTransform = useUnityTransform;
                        graspings.Add(grasping);
                    }
                }
            }
        }

        private void Start()
        {

        }

        /// <summary>
        /// Sets off a coroutine to go through all of the graspings that 
        /// this component created, as it takes a frame to check whether
        /// a grasp was successful
        /// </summary>
        public void StartGrasp()
        {
            if (isActiveAndEnabled)
                _coroutine = StartCoroutine(CoroutineGrasp());
        }

        private IEnumerator CoroutineGrasp()
        {
            foreach (var grasp in graspings)
            {
                if (!grasp.enabled) continue;
                if (requireTouch && !rigid.DiDCollide(grasp.graspedObject)) continue;

                Debug.LogWarning("Attempting Grasp GraspingManager");
                // It takes a frame for us to detect if the grasp
                // has generated constraints, only then do we know
                // that something has _actually_ been taken
                if (grasp.HasConstraints()) { grasp.Regrasp(); }
                else { grasp.StartGrasp(); }

                yield return null;

                if (grasp.HasConstraints())
                {
                    Debug.LogWarning("Caught Something");
                    yield break;
                }
            }
        }
        public void EndGrasp()
        {
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }
            foreach (var grasp in graspings)
            {
                grasp.EndGrasp();
            }
        }

    }
}
