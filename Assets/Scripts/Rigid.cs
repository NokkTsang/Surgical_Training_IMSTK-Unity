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
    /// Use this to represent movable rigid object use this to represent movable rigid objects like forceps or scalpels.
    /// Physics and collision geometry can be assigned separately.
    /// </summary>
    /// Implements a rigid body using position based dynamics from imstk.
    /// Note there are two ways rigids will be used in the simulation, one 
    /// is as free rigid bodies like a needle, or others. The other is as 
    /// tools that are driven via a controller through a device.
    /// Currently free rigids cannot be transformed through a unity parent
    /// transform.

    public class Rigid : DynamicalModel
    {

        // These filters can accept either imstk or unity geometry input
        // and output imstk geometry
        public GeometryFilter visualGeomFilter = null;
        public bool drawVisualGeometry = false;
        public GeometryFilter physicsGeomFilter = null;
        public bool drawPhysicsGeometry = false;
        public GeometryFilter collisionGeomFilter = null;
        public bool drawCollisionGeometry = false;

        public double mass = 1.0;

        public Vector3[] inertia = new Vector3[3] {
            new Vector3(1.0f, 0.0f, 0.0f),
            new Vector3(0.0f, 1.0f, 0.0f),
            new Vector3(0.0f, 0.0f, 1.0f)
            };


        public bool useBodyDamping = false;
        public double linearDampingCoeff = 0.01;
        public double angularDampingCoeff = 0.01;

        private Imstk.PbdBody _body;
        private Imstk.PbdModel _model;

        private List<Imstk.Geometry> _attachedGeometry = new List<Imstk.Geometry>();

        protected override void OnImstkInit()
        {
            if (imstkObject != null) return;

            if (physicsGeomFilter == null)
            {
                Debug.LogError("Rigid " + gameObject.name + " needs physics geometry. Deactiving");
                gameObject.SetActive(false);
                return;
            }

            imstkObject = InitObject();
            SimulationManager.sceneManager.getActiveScene().addSceneObject(imstkObject);
            InitGeometry();
            //InitGeometryMaps();
            ProcessBoundaryConditions(gameObject.GetComponents<BoundaryCondition>());
            Configure();
        }

        protected override Imstk.CollidingObject InitObject()
        {
            Imstk.PbdObject pbdObject = new Imstk.PbdObject(GetFullName());
            _model = SimulationManager.pbdModel;
            pbdObject.setDynamicalModel(_model);

            return pbdObject;
        }

        protected override void Configure()
        {
            Imstk.PbdBody pbdBody = (imstkObject as Imstk.PbdObject).getPbdBody();
            _body = pbdBody;

            // BUG this doesn't transform a subobject into world space
            // you can't have a rigid under a gameobejct with a transform

            var position = transform.localToWorldMatrix.GetPosition().ToImstkVec();
            var orientation = transform.localToWorldMatrix.rotation.ToImstkQuat();

            //var position = new Imstk.Vec3d(transform.position.x, transform.position.y, transform.position.z);
            //var orientation = new Imstk.Quatd(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w);
            
            Imstk.Mat3d inertiaTensor = Imstk.Mat3d.Identity();
            inertiaTensor.setValue(0, 0, inertia[0][0]);
            inertiaTensor.setValue(0, 1, inertia[0][1]);
            inertiaTensor.setValue(0, 2, inertia[0][1]);

            inertiaTensor.setValue(1, 0, inertia[1][0]);
            inertiaTensor.setValue(1, 1, inertia[1][1]);
            inertiaTensor.setValue(1, 2, inertia[1][2]);

            inertiaTensor.setValue(2, 0, inertia[2][0]);
            inertiaTensor.setValue(2, 1, inertia[2][1]);
            inertiaTensor.setValue(2, 2, inertia[2][2]);

            _body.setRigid(
                    position,
                    mass,
                    orientation,
                    inertiaTensor
                );

            if (useBodyDamping)
            {
                Imstk.PbdModelConfig config = SimulationManager.pbdModel.getConfig();
                config.setBodyDamping(_body.bodyHandle, linearDampingCoeff, angularDampingCoeff);
            }

            _body.bodyGravity = !ignoreGravity;
        }

        public void Update()
        {
            if (_body == null) return;
            Vector3 bodyPosition = ((Imstk.Vec3d)_body.getRigidPosition()).ToUnityVec();
            Quaternion bodyOrientation = ((Imstk.Quatd)_body.getRigidOrientation()).ToUnityQuat();
            transform.SetPositionAndRotation(bodyPosition, bodyOrientation);

            var m = transform.localToWorldMatrix.ToMat4d();
            foreach (var geom in _attachedGeometry)
            {
                geom.setTransform(m);
                geom.updatePostTransformData();
            }
        }

        protected override void InitGeometry()
        {
            // Copy all the geometries over to iMSTK, set the transform and
            // apply later. (to avoid applying transform twice *since two
            // geometries could point to the same one*)

            // No Visual Geometry

            // Setup the collision geometry
            if (collisionGeomFilter != null)
            {
                Imstk.Geometry colGeom = GetCollidingGeometry();
                // Only apply the scale to world scale, the rest is 
                // handled in the pbdRigid 
                var scale = collisionGeomFilter.transform.localToWorldMatrix.lossyScale;
                colGeom.scale(scale.ToImstkVec(), Imstk.Geometry.TransformType.ApplyToData);
                colGeom.setTransform(Imstk.Mat4d.Identity());
                imstkObject.setCollidingGeometry(colGeom);
            }
            else
            {
                Debug.LogWarning("No collision geometry provided to DynamicalModel on object " + gameObject.name);
            }

            // Setup the physics geometry
            if (physicsGeomFilter != null)
            {
                Imstk.Geometry physicsGeom = GetPhysicsGeometry();
                if (physicsGeomFilter != collisionGeomFilter)
                {
                    var scale = collisionGeomFilter.transform.localToWorldMatrix.lossyScale;
                    physicsGeom.scale(scale.ToImstkVec(), Imstk.Geometry.TransformType.ApplyToData);
                    physicsGeom.setTransform(Imstk.Mat4d.Identity());
                }
                (imstkObject as Imstk.DynamicObject).setPhysicsGeometry(physicsGeom);
                //(imstkObject as Imstk.DynamicObject).getDynamicalModel().setModelGeometry(physicsGeom);
            }
            //         else
            //         {
            //             Debug.LogError("No physics geometry provided to PbdRigidModel on object " + gameObject.name);
            //         }
            gameObject.transform.SetParent(null, false);
        }

        public void OverridePositionAndOrientation(Vector3 pos, Quaternion rot)
        {
            _body.overrideRigidPositionAndOrientation(pos.ToImstkVec(), rot.ToImstkQuat());
        }

        public void OverrideVelocities(Vector3 linear, Vector3 angular)
        {
            _body.overrideLinearAndAngularVelocity(linear.ToImstkVec(), angular.ToImstkVec());
        }

        public void ClearVelocities()
        {
            _body.overrideLinearAndAngularVelocity(Vector3.zero.ToImstkVec(), Vector3.zero.ToImstkVec());
        }

        public void Attach(Imstk.Geometry geom)
        {
             _attachedGeometry.Add(geom);
        }

        public void Detach(Imstk.Geometry geom)
        {
            _attachedGeometry.Remove(geom);
        }

        public override Imstk.Geometry GetVisualGeometry()
        {
            return visualGeomFilter != null ? visualGeomFilter.GetOutputGeometry() : null;
        }
        public override Imstk.Geometry GetPhysicsGeometry()
        {
            return physicsGeomFilter != null ? physicsGeomFilter.GetOutputGeometry() : null;
        }
        public override Imstk.Geometry GetCollidingGeometry()
        {
            return collisionGeomFilter != null ? collisionGeomFilter.GetOutputGeometry() : null;
        }

        public override ImstkUnity.Geometry GetUnityColisionGeometry()
        {
            return collisionGeomFilter != null ?  collisionGeomFilter.inputImstkGeom : null;
        }
    }
}
