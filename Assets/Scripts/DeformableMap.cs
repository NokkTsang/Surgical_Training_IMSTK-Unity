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
    /// <summary>
    /// Allows the use of separate meshes for the deformable, visual and collision representation.
    /// </summary>
    /// Will move the vertices of the target mesh according to matching points on the source mesh.
    /// The points do not have to completely coincide. In almost all cases you will need to map
    /// FROM the physics mesh TO the visual mesh, and FROM the physics mesh TO the collision mesh.
    [AddComponentMenu("iMSTK/DeformableMap")]
    public class DeformableMap : GeometryMap
    {
        public bool forceOneOne = false;
        public float oneOneTolerance = 1e-4f;
        protected override Imstk.GeometryMap MakeMap()
        {
            Imstk.Geometry parent = parentGeom.GetOutputGeometry();
            Imstk.Geometry child = childGeom.GetOutputGeometry();

            if (parentGeom == null || childGeom == null)
            {
                Debug.LogError("GeometryMap: can't create map when one or more inputs is null");
            }
            
            if (!forceOneOne && (parent.getTypeName() == Imstk.TetrahedralMesh.getStaticTypeName()
                || child.getTypeName() == Imstk.TetrahedralMesh.getStaticTypeName()))
            {
                Debug.Log("Trying to make tetrahedral map with " + parent.getTypeName() + " and " + child.getTypeName());
                var map = new Imstk.PointToTetMap(parent, child);
                map.compute();
                return map;
            }
            else
            {
                Debug.Log("Trying to make one to one map with " + parent.getTypeName() + " and " + child.getTypeName());
                var map = new Imstk.PointwiseMap(parent, child);
                // Tolerance needed to avoid issues with double/float conversions
                map.setTolerance(oneOneTolerance);
                map.compute();
                return map;
            }
        }
    }
}