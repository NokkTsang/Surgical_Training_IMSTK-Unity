using System;
using Imstk;
using UnityEngine;

namespace ImstkUnity
{

// There is some overlap between this class and "Constrain Deformables" the intent for this
// is to have two surfaces that are a very close match with regard to their meshes e.g. one 
// mesh is a scaled up version of the other
public class ConstrainClosestPoints : ImstkBehaviour
    {
        public Deformable objectA;
        public Deformable objectB;
        public double stiffness = 1;
        [Range(0,1000)]
        public double maxDist = 1000;
        [SerializeField] private int _constraintCount = 0;

        protected override void OnImstkStart()
        {
            if (objectA == null || !objectA.isActiveAndEnabled || objectB == null || !objectB.isActiveAndEnabled)
            {
                enabled = false;
                return;
            }
            
            Constrain(objectA.GetDynamicObject() as PbdObject, objectB.GetDynamicObject() as PbdObject);
        }

        private void Constrain( PbdObject pbdObjectA, PbdObject pbdObjectB)
        {
            Imstk.Geometry geomA = pbdObjectA.getPhysicsGeometry();
            var pointsA = Utils.CastTo<PointSet>(geomA);
            var handleA = pbdObjectA.getPbdBody().bodyHandle;
            

            Imstk.Geometry geomB = pbdObjectB.getPhysicsGeometry();
            var pointsB = Utils.CastTo<PointSet>(geomB);
            var handleB = pbdObjectB.getPbdBody().bodyHandle;
            
            var verticesA = MathUtil.ToVector3Array(pointsA.getVertexPositions());
            var verticesB = MathUtil.ToVector3Array(pointsB.getVertexPositions());
            
            for (var i = 0; i < verticesA.Length; ++i)
            {
                var minDist = double.MaxValue;
                var minIndex = -1;
                var vertexA = verticesA[i];
                for (var j = 0; j < verticesB.Length; ++j)
                {
                    var vertexB = verticesB[j];
                    var dist = (vertexB - vertexA).sqrMagnitude;
                    
                    if (!(dist < minDist)) continue;
                    
                    minDist = dist;
                    minIndex = j;
                }

                if (minIndex == -1) continue;
                
                var constraint = new Imstk.PbdDistanceConstraint();
                var p1 = new Imstk.IntPair(handleA, i);
                var p2 = new Imstk.IntPair(handleB, minIndex);
                constraint.initConstraint(Math.Sqrt(minDist), p1, p2, stiffness);
                SimulationManager.pbdModel.getConstraints().addConstraint(constraint);
                _constraintCount++;
            }
            
        }
    }
}