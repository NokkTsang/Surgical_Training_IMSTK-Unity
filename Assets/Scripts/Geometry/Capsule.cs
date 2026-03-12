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
    public class Capsule : Geometry
    {
        public Vector3 center = Vector3.zero;
        public float radius = 0.5f;
        public Quaternion orientation = Quaternion.identity;
        public float length = 1.0f;

        public Capsule()
        {
            geomType = GeometryType.Capsule;
        }

        public Vector3 GetTransformedCenter(Transform transform)
        {
            return transform.TransformPoint(center);
        }

        public Quaternion GetTransformedOrientation(Transform transform)
        {
            return orientation * transform.rotation;
        }

        public Mesh GetMesh()
        {
            Imstk.Capsule geom = this.ToImstkGeometry() as Imstk.Capsule;
            Imstk.SurfaceMesh surfMesh = Imstk.Utils.toSurfaceMesh(geom);
            return surfMesh.ToMesh();
        }
    }
}