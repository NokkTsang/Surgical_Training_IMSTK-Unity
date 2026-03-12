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
    public class Sphere : Geometry
    {
        public Vector3 center = Vector3.zero;
        public float radius = 0.5f;

        public Sphere()
        {
            geomType = GeometryType.Sphere;
        }

        public Vector3 GetTransformedCenter(Transform transform)
        {
            return transform.TransformPoint(center);
        }

        public float GetTransformedRadius(Transform transform)
        {
            Vector3 localScale = transform.localScale;
            float max = Mathf.Max(Mathf.Max(localScale.x, localScale.y), localScale.z);
            return radius * max;
        }

        public Mesh GetMesh()
        {
            Imstk.Sphere geom = this.ToImstkGeometry() as Imstk.Sphere;
            Imstk.SurfaceMesh surfMesh = Imstk.Utils.toUVSphereSurfaceMesh(geom, 7, 7);
            return surfMesh.ToMesh();
        }
    }
}