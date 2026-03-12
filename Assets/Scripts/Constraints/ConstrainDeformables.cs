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

using System;
using System.Collections.Generic;
using Imstk;
using UnityEngine;

namespace ImstkUnity
{
    /// <summary>
    /// This will set up a set of distance constraints between two deformable objects.
    /// </summary>
    /// The constraints will be limited to the area encompassed by the assigned mesh
    /// constraints will be generated for _all_ pairs of points whose distance is smaller than
    /// or equal the cutoff distance.The length of the constraint will be set to
    /// the original distance * restLength. Use this if you want to attach a deformable
    /// to another deformable. E.g. a vessel to another organ.
    public class ConstrainDeformables : ConstraintGenerator
    {
        public Deformable objectA = null;
        public Deformable objectB = null;

        [Tooltip("Area that is searched for points")]
        public MeshFilter constrainedArea = null;
        [Tooltip("Hides the constraining mesh")]
        public bool hideMesh = true;

        [Tooltip("Ignore points that are farther apart than `cutoff`")]
        public float cutoff = 0.01f;

        [Tooltip("Affects the resting length of the constraint")]
        [Min(0)]
        public float restLengthFactor = 0.0f;

        [Tooltip("How stiff this constraint should be")]
        [Min(0)]
        public float stiffness = 1.0e5f;

        [SerializeField] private int _constraintCount = 0;
        private List<PbdConstraint> _constraints = new List<PbdConstraint>();
        public override List<PbdConstraint> Constraints => _constraints;
        
        void Start()
        {
            var renderer = constrainedArea.gameObject.GetComponent<MeshRenderer>();
            if (renderer != null && hideMesh)
            {
                renderer.enabled = false;
            }
        }

        // Needs to run after objectA and deformable have been initialized

        protected override void OnImstkStart()
        {
            if (!isActiveAndEnabled) return;
            if (objectA == null || !objectA.isActiveAndEnabled || objectB == null || !objectB.isActiveAndEnabled ||
    constrainedArea == null)
            {
                enabled = false;
                Debug.Log(name + " disabled due to missing or disabled dependency.");
                return;
            }
                
            ImstkMesh bcGeometry = constrainedArea.sharedMesh.ToImstkMesh(constrainedArea.transform.localToWorldMatrix);
            Imstk.SurfaceMesh bcImstkGeometry = bcGeometry.ToImstkGeometry() as Imstk.SurfaceMesh;

            Imstk.Geometry geomA = (objectA.GetDynamicObject() as Imstk.PbdObject).getPhysicsGeometry();
            var pointsA = GeomUtil.PointsInside(bcImstkGeometry, Imstk.Utils.CastTo<Imstk.PointSet>(geomA));

            Imstk.Geometry geomB = (objectB.GetDynamicObject() as Imstk.PbdObject).getPhysicsGeometry();
            var pointsB = GeomUtil.PointsInside(bcImstkGeometry, Imstk.Utils.CastTo<Imstk.PointSet>(geomB));

            if (pointsA.Count == 0 || pointsB.Count == 0) 
            {    
                if (pointsA.Count == 0)
                {
                    Debug.LogWarning($"No points of object {objectA.name} in the constraint area {gameObject.name}");
                }

                if (pointsB.Count == 0)
                {
                    Debug.LogWarning($"No points of object {objectB.name} in the constraint area {gameObject.name}");
                }

                return;
            }
            
            ConstrainPoints(objectA, pointsA, objectB, pointsB);
        }

        private void ConstrainPoints(Deformable objectA, List<uint> pointsA, Deformable objectB, List<uint> pointsB)
        {
            _constraintCount = 0;
            // NOTE HS - 20230215 need to investigate type conversion for result of .getVertexPosition() etc
            // returns a wrapped swig type rather than Vec3d for example
            Imstk.Geometry geomA = (objectA.GetDynamicObject() as Imstk.PbdObject).getPhysicsGeometry();
            var pointSetA = Imstk.Utils.CastTo<Imstk.PointSet>(geomA);
            var verticesA = MathUtil.ToVector3Array(pointSetA.getVertexPositions());



            Imstk.Geometry geomB = (objectB.GetDynamicObject() as Imstk.PbdObject).getPhysicsGeometry();
            var pointSetB = Imstk.Utils.CastTo<Imstk.PointSet>(geomB);
            var verticesB = MathUtil.ToVector3Array(pointSetB.getVertexPositions());

            foreach (var indexA in pointsA)
            {
                var pointA = verticesA[indexA];
                foreach(var indexB in pointsB)
                {
                    var pointB = verticesB[indexB];
                    var dist = (pointB - pointA).sqrMagnitude;
                    if (dist <= cutoff)
                    {
                        var constraint = new Imstk.PbdDistanceConstraint();
                        var p1 = new Imstk.IntPair((objectA.GetDynamicObject() as Imstk.PbdObject).getPbdBody().bodyHandle, (int)indexA);
                        var p2 = new Imstk.IntPair((objectB.GetDynamicObject() as Imstk.PbdObject).getPbdBody().bodyHandle, (int)indexB);
                        constraint.initConstraint(Math.Sqrt(dist) * restLengthFactor, p1, p2);
                        SimulationManager.pbdModel.getConstraints().addConstraint(constraint);
                        _constraints.Add(constraint);
                        _constraintCount++;
                    }
                }

            }
            Debug.Log($"{name}: added {_constraintCount} constraints.");
        }

    }
}

