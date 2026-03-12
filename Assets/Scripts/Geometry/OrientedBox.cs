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
    public class OrientedBox : Geometry
    {
        public Vector3 center = Vector3.zero;
        public Vector3 extents = new Vector3(0.5f, 0.5f, 0.5f);
        public Quaternion orientation = Quaternion.identity;

        public OrientedBox()
        {
            geomType = GeometryType.OrientedBox;
        }

        public Vector3 GetTransformedCenter(Transform transform)
        {
            return transform.TransformPoint(center);
        }

        public Vector3 GetTransformedExtent(Transform transform)
        {
            return Vector3.Scale(extents, transform.localScale);
        }

        public Quaternion GetTransformedOrientation(Transform transform)
        {
            return orientation * transform.rotation;
        }

        public Mesh GetMesh()
        {
            Imstk.OrientedBox geom = this.ToImstkGeometry() as Imstk.OrientedBox;
            Imstk.SurfaceMesh surfMesh = Imstk.Utils.toSurfaceMesh(geom);
            return surfMesh.ToMesh();
        }
    }
}