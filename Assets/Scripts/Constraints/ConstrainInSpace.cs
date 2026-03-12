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
using System.Collections.Generic;
using UnityEngine;

namespace ImstkUnity
{
    /// <summary>
    /// This will set up a set of distance constraints between the points of deformable
    /// that are found inside the constrained area and virtual points, effectively 
    /// attaching the deformable to those points in space. 
    /// </summary>   
    /// The constraints will be limited to the area encompassed by the assigned mesh
    /// constraints will be generated for _all_ points. The length of the constraint will be set to 
    /// restLength. Use this if you want to attach a deformable to a point in space 
    /// e.g. Suspend an organ in the body cavity.
    public class ConstrainInSpace : ConstraintGenerator
    {
        public Deformable deformable = null;

        [Tooltip("Area that is searched for points")]
        public MeshFilter constrainedArea = null;
        [Tooltip("Hides the constraining mesh")]
        public bool hideMesh = true;

        [Tooltip("Affects the resting length of the constraint")]
        [Min(0)]
        public float restLength = 0.0f;

        public float stiffness = 1.0e5f;

        public bool runOnStart = true;

        [SerializeField] private int _constraintCount = 0;
        List<Imstk.PbdConstraint> _constraints = new List<Imstk.PbdConstraint>();

        public override List<PbdConstraint> Constraints => _constraints;

        // Start is called before the first frame update
        void Start()
        {
            var renderer = constrainedArea.gameObject.GetComponent<MeshRenderer>();
            if (renderer != null && hideMesh)
            {
                renderer.enabled = false;
            }
        }


        protected override void OnImstkStart()
        {
            if (!isActiveAndEnabled) return;
            if (deformable == null || !deformable.isActiveAndEnabled ||
    constrainedArea == null)
            {
                enabled = false;
                Debug.Log(name + " disabled due to missing or disabled dependency.");
                return;
            }
            
            if (runOnStart)
            {
                Constrain();
            }
        }

        public void Constrain()
        {
            ImstkMesh bcGeometry = constrainedArea.sharedMesh.ToImstkMesh(constrainedArea.transform.localToWorldMatrix);
            Imstk.SurfaceMesh bcImstkGeometry = bcGeometry.ToImstkGeometry() as Imstk.SurfaceMesh;

            Imstk.Geometry geom = (deformable.GetDynamicObject() as Imstk.PbdObject).getPhysicsGeometry();
            var points = PointsInside(bcImstkGeometry, Imstk.Utils.CastTo<Imstk.PointSet>(geom));

            if (points.Count == 0)
            {
                Debug.LogWarning("No points in constraint area " + gameObject.name);
            }

            ConstrainPoints(deformable, points);
        }

        public void Release()
        {
            var model = SimulationManager.pbdModel;
            var constraints = model.getConstraints();
            foreach (var constraint in _constraints)
            {
                constraints.removeConstraint(constraint);
            }
        }

        // Refactor to common functions
        List<uint> PointsInside(SurfaceMesh enclosingMesh, PointSet sampleMesh)
        {
            var result = new List<uint>();
            // Compute mask of enclosed points
            Imstk.SelectEnclosedPoints selectEnclosed = new Imstk.SelectEnclosedPoints();
            selectEnclosed.setInputMesh(enclosingMesh);
            selectEnclosed.setInputPoints(sampleMesh);
            selectEnclosed.setUsePruning(false);
            selectEnclosed.update();
            
            Imstk.DataArrayuc isInside = selectEnclosed.getIsInsideMask();
            byte[] isInsideBytes = new byte[isInside.size()];
            isInside.getValues(isInsideBytes);
            for (int i = 0; i < isInsideBytes.Length; i++)
            {
                if (isInsideBytes[i] == 1)
                    result.Add((uint)i);
            }

            return result;
        }

        private void ConstrainPoints(Deformable deformable, List<uint> points)
        {
            _constraintCount = 0;
            Imstk.Geometry geom = (deformable.GetDynamicObject() as Imstk.PbdObject).getPhysicsGeometry();
            var pointSet = Imstk.Utils.CastTo<Imstk.PointSet>(geom);
            var vertices = MathUtil.ToVector3Array(pointSet.getVertexPositions());
            var model = SimulationManager.pbdModel;
            Vec3d zero = new Vec3d(0, 0, 0);
            foreach(var indexB in points)
            {
                var pointB = vertices[indexB].ToImstkVec();
                var p1 = model.addVirtualParticle(pointB, 0, zero, true);
                var constraint = new Imstk.PbdDistanceConstraint();
                var p2 = new Imstk.IntPair((deformable.GetDynamicObject() as Imstk.PbdObject).getPbdBody().bodyHandle, (int)indexB);
                constraint.initConstraint(restLength, p1, p2);
                constraint.setStiffness(stiffness);
                model.getConstraints().addConstraint(constraint);
                _constraints.Add(constraint);
                _constraintCount++;
            }
            Debug.Log($"{name}: added {_constraintCount} constraints.");
        }

    }
}

