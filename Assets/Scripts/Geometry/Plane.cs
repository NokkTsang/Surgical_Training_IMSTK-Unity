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
    public class Plane : Geometry
    {
        public Plane()
        {
            geomType = GeometryType.Plane;
        }

        public Vector3 GetTransformedCenter(Transform transform)
        {
            return transform.TransformPoint(center);
        }

        public Vector3 GetTransformedNormal(Transform transform)
        {
            return transform.TransformDirection(normal).normalized;
        }

        public Vector3 center = Vector3.zero;
        public Vector3 normal = Vector3.up;
        public float visualWidth = 1.0f; ///> Purely used for visualization
    }
}