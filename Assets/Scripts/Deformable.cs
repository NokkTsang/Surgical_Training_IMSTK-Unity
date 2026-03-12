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
using UnityEngine;

namespace ImstkUnity
{ 
    /// <summary>
    /// Use this to represent deformable objects. Position Based Dynamics (PBD),
    /// is used to model the deformation.
    /// </summary>
    /// This model supports Lines (1D), Surface Meshes (2D) and, Tetrahedral Meshes (3D)
    /// dynamical models see the iMSTK Documentation <https://imstk.gitlab.io/Dynamical_Models/PbdModel.html>
    /// for more information on constraints and models. Visual, physics and collision 
    /// geometry can be assigned separately. If you do, a separate map will be necessary 
    /// to update the various meshes. 
    /// The physics geometry determines the type of constraint that can be used, 
    /// an invalid constraint may cause problems.
    /// | Physics Geometry Type | Valid Constraints |
    /// | --------------------- | ----------------- |
    /// | Line Mesh(Threads)    | Distance Stiffness, Bend Stiffness |
    /// | Surface Mesh(Membranes, Bags) | Distance Stiffness, Dihedral Stiffness, Area Stiffness |
    /// | Volumetric Mesh(Tissue) | Distance Stiffness, Volume Stiffness, Fem(all models) |
    [AddComponentMenu("iMSTK/Deformable")]
    public class Deformable : DeformableModel
    {
        public double distanceStiffness = 100.0;
        public bool useDistanceConstraint = true;

        public double dihedralStiffness = 10.0;
        public bool useDihedralConstraint = true;

        public double areaStiffness = 10.0;
        public bool useAreaConstraint = true;

        public double bendStiffness = 10.0;
        public int maxBendStride = 2;
        public bool useBendConstraint = false;

        public double volumeStiffness = 10.0;
        public bool useVolumeConstraint = false;

        public double youngsModulus = 5000.0;
        public double possionsRatio = 0.4;
        public double mu = 0.0;
        public double lambda = 0.0;
        public double viscousDampingCoeff = 0.01;
        public bool useFEMConstraint = false;
        public Imstk.PbdFemConstraint.MaterialType materialType = Imstk.PbdFemConstraint.MaterialType.StVK;

        public double uniformMassValue = 1.0;
 
        public HashSet<int> fixedIndices = new HashSet<int>();

        public bool useBodyDamping = false;
        public double linearDampingCoeff = 0.01;
        public double angularDampingCoeff = 0.01;

        protected override Imstk.CollidingObject InitObject()
        {
            Imstk.PbdObject pbdObject = new Imstk.PbdObject(GetFullName());
            pbdObject.setDynamicalModel(SimulationManager.pbdModel);
            return pbdObject;
        }

        protected override void Configure()
        {
            Imstk.PbdModelConfig config = SimulationManager.pbdModel.getConfig();
            Imstk.PbdBody pbdBody = (imstkObject as Imstk.PbdObject).getPbdBody();
            int bodyHandle = pbdBody.bodyHandle;

            if (useDistanceConstraint)
                config.enableConstraint(Imstk.PbdModelConfig.ConstraintGenType.Distance, distanceStiffness, bodyHandle);

            if (useDihedralConstraint)
                config.enableConstraint(Imstk.PbdModelConfig.ConstraintGenType.Dihedral, dihedralStiffness, bodyHandle);

            if (useAreaConstraint)
                config.enableConstraint(Imstk.PbdModelConfig.ConstraintGenType.Area, areaStiffness, bodyHandle);

            if (useVolumeConstraint)
                config.enableConstraint(Imstk.PbdModelConfig.ConstraintGenType.Volume, volumeStiffness, bodyHandle);

            if (useBendConstraint)
            {
                for (int i = 1; i <= maxBendStride; i++)
                {
                    config.enableBendConstraint(bendStiffness, i, true, bodyHandle);
                }
            }

            if (useFEMConstraint)
            {
                if ((imstkObject as Imstk.DynamicObject).getPhysicsGeometry().getTypeName() != "TetrahedralMesh")
                {
                    Debug.Log("Currently only Tetrahedral mesh is supported for FEM constraints");
                }
                else
                {
                    config.enableFemConstraint(materialType, youngsModulus, possionsRatio, bodyHandle);
                }
            }

            if (useBodyDamping)
            {
                config.setBodyDamping(bodyHandle, linearDampingCoeff, angularDampingCoeff);
            }

            pbdBody.uniformMassValue = uniformMassValue;
            pbdBody.bodyGravity = !ignoreGravity;

            var fixedNodes = new Imstk.VectorInt();
            foreach (int id in fixedIndices)
            {
                fixedNodes.Add(id);
            }
            pbdBody.fixedNodeIds = fixedNodes;
        }

        protected override void ProcessBoundaryConditions(BoundaryCondition[] conditions)
        {
            // This model currently uses fixed vertices for BC, this is what we will compute
            // Clear them to start with
            fixedIndices.Clear();
            if (conditions.Length == 0)
                return;

            // For every BC test intersection to find fixed vertices
            foreach (BoundaryCondition condition in conditions)
            {
                if (!condition.enabled) return;
                // Create the surface mesh the points will be tested if inside of
                MeshFilter meshFilter = condition.bcObj.GetComponent<MeshFilter>();
                Transform transform = condition.bcObj.GetComponent<Transform>();
                if (meshFilter == null)
                    continue;

                // Convert unity to imstk geometry
                ImstkMesh bcGeometry = meshFilter.sharedMesh.ToImstkMesh(transform.localToWorldMatrix);
                Imstk.SurfaceMesh bcImstkGeometry = bcGeometry.ToImstkGeometry() as Imstk.SurfaceMesh;

                // Compute mask of enclosed points
                Imstk.SelectEnclosedPoints selectEnclosed = new Imstk.SelectEnclosedPoints();
                selectEnclosed.setInputMesh(bcImstkGeometry);
                Imstk.Geometry physicsGeom = (imstkObject as Imstk.PbdObject).getPhysicsGeometry();
                selectEnclosed.setInputPoints(Imstk.Utils.CastTo<Imstk.PointSet>(physicsGeom));
                selectEnclosed.setUsePruning(false);
                selectEnclosed.update();

                Imstk.DataArrayuc isInside = selectEnclosed.getIsInsideMask();
                byte[] isInsideBytes = new byte[isInside.size()];
                isInside.getValues(isInsideBytes);
                for (int i = 0; i < isInsideBytes.Length; i++)
                {
                    if (isInsideBytes[i] == 1)
                        fixedIndices.Add(i);
                }
            }
        }
    }
}