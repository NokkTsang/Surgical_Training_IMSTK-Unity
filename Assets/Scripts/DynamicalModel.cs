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
    [RequireComponent(typeof(Transform))]
    public abstract class DynamicalModel : ImstkBehaviour
    {
        // HS 20221220 Need to figure out what we need at this level
        // - What is common setup, what can just move into Deformable/Rigid
        // - What distinguishes an object that can collied to one that doesn't (this is 
        //   needed for setting up the collision handling object)

        // Components
        protected MeshFilter meshFilter = null;

        protected Imstk.CollidingObject imstkObject;

        // Indicates that geometry changes may happen as opposed to just
        // position changes
        // If true update needs to refresh vertex and triangle indices rather
        // than just copying positions
        public bool dynamicGeometry = false;

        // Tells the object to ignore the global gravity parameter
        public bool ignoreGravity = false;

        private Dictionary<DynamicalModel, Imstk.CollisionInteraction> _colliders = new Dictionary<DynamicalModel, Imstk.CollisionInteraction>();

        public List<Imstk.CollisionInteraction> collisionInteractions = new List<Imstk.CollisionInteraction>();

#if UNITY_EDITOR
        public bool _collisionPanelOpen = false;
        public Dictionary<DynamicalModel, bool> _subPanelOpen = new Dictionary<DynamicalModel, bool>();
#endif

        /// <summary>
        /// Get the pointer to the object in c (not available until after initialize)
        /// </summary>
        /// <returns></returns>
        public Imstk.CollidingObject GetDynamicObject() { return imstkObject; }

        // NOTE HS 20221220 Refactor readonly properties, initialize in each object 
        public abstract Imstk.Geometry GetVisualGeometry();
        public abstract Imstk.Geometry GetPhysicsGeometry();
        public abstract Imstk.Geometry GetCollidingGeometry();

        // Editor support creating the imstk geometry can be expensive
        // use this to determine if the user has assigned a collision geometry
        // TODO maybe refactor geoemtry filter field
        public abstract ImstkUnity.Geometry GetUnityColisionGeometry();

        protected abstract Imstk.CollidingObject InitObject();

        protected abstract void Configure();

        /// <summary>
        /// Each subclassed model may *apply* boundary conditions differently
        /// </summary>
        /// <param name="conditions">All the conditions to be processed</param>
        protected virtual void ProcessBoundaryConditions(BoundaryCondition[] conditions) { }

        protected abstract void InitGeometry();


        public void AddCollisionWith(DynamicalModel other, Imstk.CollisionInteraction collision)
        {
            if (_colliders.ContainsKey(other))
            {
                Debug.LogWarning(name + " has already a collision with " + other.name + " overwriting it");
            }
            _colliders[other] = collision;
        }

        public Imstk.CollisionInteraction GetCollisionInteractionWith(DynamicalModel other)
        {
            if (_colliders.ContainsKey(other))
            {
                return _colliders[other];
            }
            return null;
        }

        public bool DiDCollide(DynamicalModel other)
        {
            return imstkObject.didCollide(other.imstkObject);
        }

        public Imstk.VectorCollisionData GetCollisionData(DynamicalModel other)
        {
            return imstkObject.getCollisions(other.imstkObject);
        }
        
        public List<int> CollisionTriangles(DynamicalModel other)
        {

            var result = new List<int>();
            var otherImstkObj = other.GetDynamicObject();
            var data = imstkObject.getCollisions(otherImstkObj);
            if (data.Count == 0) return result;
 
            var otherImstkGeom = Imstk.Utils.CastTo<Imstk.SurfaceMesh>(otherImstkObj.getCollidingGeometry());
            if (otherImstkGeom == null)
            {
                Debug.LogWarning("CollisionTriangles only works when checking for collisions with surface meshes");
                return result;
            }

            // Note that his only works for non-compound geometries, for compound geometries `geomA` and `geomB` will represent one
            // of the subgeometries


//             Debug.Log("Col " + otherImstkGeom.getName() + " " + otherImstkGeom.getGlobalId());
//             Debug.Log("Phy " + otherImstkGeom.getName() + " " + otherImstkGeom.getGlobalId());

            // Note that his only works for non-compound geometries, for compound geometries `geomA` and `geomB` will represent one
            // of the subgeometries
            // TODO oddly the incoming geometries are "unnamed" while the other geometry has a name, don't really
            // know where the name is lost but the Id seems to be ok



//             Debug.Assert(data[0].geomA.getGlobalId() == otherImstkGeom.getGlobalId() ||
//                  data[0].geomB.getGlobalId() == otherImstkGeom.getGlobalId(), "Can't find `other` geom in collision data");


            // Collision Data has an "A" and a "B" side, we need to figure out which side to check to pull out the colliding 
            // triangles
            bool otherIsA = data[0].geomA.getGlobalId() == otherImstkGeom.getGlobalId();
            
            foreach (var item in data)
            {
//                 Debug.Log("A " + item.geomA.getName() + " " + data[0].geomA.getGlobalId());
//                 Debug.Log("B " + item.geomB.getName() + " " + data[0].geomB.getGlobalId());

                var elements = (otherIsA) ? item.elementsA : item.elementsB;
                foreach (Imstk.CollisionElement el in elements)
                {
                    switch (el.type)
                    {
                        case Imstk.CollisionElementType.CellIndex:
                            {
                                var element = el.element.m_CellIndexElement;
                                if (element != null)
                                {
                                    result.Add(element.parentId);
                                }
                                var vertexIndices = otherImstkGeom.getTriangleIndices()[(uint)element.parentId];

                                Imstk.Vec3d v1 = otherImstkGeom.getVertexPosition((uint)vertexIndices[0]);
                                Imstk.Vec3d v2 = otherImstkGeom.getVertexPosition((uint)vertexIndices[1]);
                                Imstk.Vec3d v3 = otherImstkGeom.getVertexPosition((uint)vertexIndices[2]);

                                Debug.DrawLine(v1.ToUnityVec(), v2.ToUnityVec(), Color.green);
                                Debug.DrawLine(v2.ToUnityVec(), v3.ToUnityVec(), Color.green);
                                Debug.DrawLine(v3.ToUnityVec(), v1.ToUnityVec(), Color.green);

                                break;
                            }
                            // Skip other types for now, SurfaceMesh collisions 
                            // should generally return CellIndexElements
                    }
                }
            }

            return result;
        }
    };
}