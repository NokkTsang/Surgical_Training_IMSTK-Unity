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

using Imstk;
using UnityEngine;

namespace ImstkUnity
{

    [RequireComponent(typeof(MeshFilter))]
    /// <summary>
    /// Assigns a set of strain parameters to a deformable object based on a mesh
    /// Overlapping regions will be resolved in random order.
    /// </summary>
    /// This is used to assign different material properties to different regions of a deformable object.
    /// in effect creating an anisotropic material. At the moment this is only supported for tetrahedral meshes
    /// and strain parameters
    public class StrainParameter : ImstkBehaviour
    {
        public MeshFilter region;
        public Deformable deformable;
        public bool hideRegion = true;

        public Imstk.PbdFemConstraint.MaterialType materialType;
        public double  youngsModulus = 5000.0;
        public double possionsRatio = 0.4;

        private void Start()
        {
            Debug.Assert(region != null);
            Debug.Assert(deformable != null);
            var renderer = region.gameObject.GetComponent<MeshRenderer>();
            if (renderer != null && hideRegion)
            {
                renderer.enabled = false;
            }
        }

        protected override void OnImstkInit()
        {
            // Make sure the deformable is initialized
            deformable.ImstkInit();

            var geom = deformable.GetPhysicsGeometry();
            if (geom.getTypeName() != Imstk.TetrahedralMesh.getStaticTypeName())
            {
                Debug.LogError("StrainParameter: Deformable is not a TetrahedralMesh");
                return;
            }

            // Initialize the strain parameters attribute if necessary
            // Material -1 will be ignored
            Imstk.TetrahedralMesh tetMesh = Imstk.Utils.CastTo<Imstk.TetrahedralMesh>(geom);
            var strainParams = tetMesh.getStrainParameters();
            if (strainParams == null)
            {
                strainParams = new VecDataArray3d(tetMesh.getNumCells());
                for (uint i = 0; i < tetMesh.getNumCells(); i++)
                {
                    strainParams[i] = new Vec3d(-1, 0, 0);
                }
            }
            
            var pointSet = Imstk.Utils.CastTo<Imstk.PointSet>(tetMesh);
            var pointsInside = GeomUtil.PointsInside(region, pointSet);
            double materialNumber = (double)materialType;
            foreach (var point in pointsInside)
            {
                if (point >= 0)
                {
                    var cells = tetMesh.getCellsForVertex((int)point);
                    foreach (var cell in cells)
                    {
                        strainParams[(uint)cell] = new Vec3d(materialNumber, youngsModulus, possionsRatio);
                    }
                }
            }

            tetMesh.setStrainParameters(strainParams);
        }
    }

}
