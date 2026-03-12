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
using Imstk;
using UnityEngine;
using static ImstkUnity.Constraints;

namespace ImstkUnity
{
    /// <summary>
    /// Base class for components that generate constraints, implements functionality to draw constraints
    /// as gizmos and make constraints breakable via a simple strain calculation. This should work on all
    /// body to body constraints (i.e. not internal ones that hold a body together)
    /// </summary>
    public abstract class ConstraintGenerator : ImstkBehaviour
    {
        private readonly float radius = 1f;

        
        public bool drawGizmos = false;
        
        [Tooltip("Gizmo line will be drawn between all points of the constraint, otherwise " +
                 "a spehere in the centroid will be drawn.")]
        public bool gizmosAsLines = true;

        [Tooltip("If true the constraints will be removed if the maxStrain is exceeded")]
        public bool breakable = false;
        [Tooltip("Extension over restlength, if restlength == 0, plain max length")]
        [Min(0)]
        public double maxStrain = 2; 
        
     
        public abstract List<PbdConstraint> Constraints { get; }

        public void OnDrawGizmos()
        {
            if (!drawGizmos) return;
            if (gizmosAsLines)
            {
                DrawAsLines();
            }
            else
            {
                DrawAsSpheres();
            }
        }

        private void DrawAsLines()
        {
            var pbdModel = SimulationManager.pbdModel;
            foreach (var c in Constraints)
            {
                var particles = c.getParticles();
                var p0 = pbdModel.getBody((uint)particles[0].first).vertices[(uint)particles[0].second].ToUnityVec();
                 
                for (int i = 1; i < particles.Count; ++i)
                {
                    var p1 = pbdModel.getBody((uint)particles[i].first).vertices[(uint)particles[i].second].ToUnityVec();
                    Gizmos.DrawLine(p0, p1);
                    p0 = p1;
                }
            }
        }
        
        private void Update()
        {
            if (!breakable) return;
            RemoveStrainedConstraints(Constraints, maxStrain);
        }

        private void DrawAsSpheres()
        {
            var pbdModel = SimulationManager.pbdModel;
            foreach (var c in Constraints)
            {
                var p = new Vector3(0,0,0);
                var particles = c.getParticles();
                 
                for (int i = 0; i < particles.Count; ++i)
                {
                    p += pbdModel.getBody((uint)particles[i].first).vertices[(uint)particles[i].second].ToUnityVec();
                }

                p = p / particles.Count;
                Gizmos.DrawSphere(p, radius);
            }
        }
    }
}