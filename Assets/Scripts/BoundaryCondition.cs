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

using UnityEngine;

namespace ImstkUnity
{
    [AddComponentMenu("iMSTK/BoundaryCondition")]
    /// <summary>
    /// Use this behavior to mark vertices on a deformable object as `fixed`. 
    /// </summary>
    /// This means they won't move but are still part of the overall system.
    /// In general this will mean that the object will be attached to the points 
    /// selected. As the shape assigned can be any mesh this is an easy way to fix 
    /// an object in space. To indicate which points should be you can use any unity
    /// Boundary condition points are considered to have infinite mass, this may cause
    /// issues with collision response, prefer the "Constraint" behaviors over this one.
    public class BoundaryCondition : MonoBehaviour
    {
        public GameObject bcObj = null;
        public bool hideMesh = true;
        void Start()
        {
            var renderer = bcObj.GetComponent<MeshRenderer>();
            if (renderer != null && hideMesh)
            {
                renderer.enabled = false;
            }
        }
    }
}