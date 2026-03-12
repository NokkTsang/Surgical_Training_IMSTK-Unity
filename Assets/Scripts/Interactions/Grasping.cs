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
    /// This class is used to enable grasping between a rigid object and a deformable.
    /// Assigning a rigid and a deformable, then use <c>StartGrasp()</c> and <c>EndGrasp()</c>
    /// to drive the grasping. <c>useVertexGrasping</c> indicates that the grasped item on
    /// the deformable will be a vertex, otherwise it will be a whole cell (e.g. triangle)
    /// When grasped iMSTK will attempt to maintain the relation between the grasped and the
    /// grasping geometry. When the rigid is moved the grasped side will attempt to move with
    /// it.
    /// </summary>
    public class Grasping : ImstkInteractionBehaviour
    {
        public enum GraspType
        {
            Vertex,
            Cell,
        }
        public Rigid rigidModel;
        public GeometryFilter graspingGeometry;
        public DynamicalModel graspedObject;
        public bool disableCollision = true;
        [Tooltip("When true uses the unity transform hierarchy for the grasping geom transform")]
        public bool useUnityTransform = false;

        public GraspType graspType = GraspType.Cell;

        Imstk.PbdObjectGrasping interaction;
        private string _collisionDetectionType;

        public double deformableGraspingStiffness = 0.4;
        public double rigidGraspingStiffness = 1 / 0.0001;
        bool _currentState;

        [Header("Keyboard Control")]
        public bool useKeyboardControl = false;
        public string graspKey = "g";

        public DynamicalModel GraspedObject
        {
            get
            {
                return HasConstraints() ? graspedObject : null;
            }
        }

        Imstk.CollisionInteraction _imstkCollision;
        private void OnEnable()
        {

        }

        bool OneIsA<T>(DynamicalModel a, DynamicalModel b) where T : DynamicalModel
        {
            if (b as T != null) return true;
            if (a as T != null) return true;
            return false;
        }

        public override Imstk.SceneObject GetImstkInteraction()
        {
            if (rigidModel == null || graspedObject == null)
            {
                Debug.LogWarning("Both models need to be assigned for the Grasping Interaction to work");
                enabled = false;
                return null;
            }

            if (!rigidModel.isActiveAndEnabled || !graspedObject.isActiveAndEnabled)
            {
                enabled = false;
                return null;
            }

            _collisionDetectionType = CollisionInteraction.GetCDType(rigidModel, graspedObject);

            if (_collisionDetectionType == "")
            {
                Debug.LogError("Could not determine collision type for grasping between " + gameObject.name + " and " + graspedObject.GetFullName());
            }

            interaction = new Imstk.PbdObjectGrasping(graspedObject.GetDynamicObject() as Imstk.PbdObject,
                rigidModel.GetDynamicObject() as Imstk.PbdObject);

            interaction.setStiffness(deformableGraspingStiffness);
            interaction.setCompliance(1 / rigidGraspingStiffness);

            Imstk.Geometry geom = rigidModel.GetDynamicObject().getCollidingGeometry();

            Imstk.AnalyticalGeometry analytical = Imstk.Utils.CastTo<Imstk.AnalyticalGeometry>(geom);

            if (analytical == null)
            {
                Debug.LogError("Can't convert to analytical geometry" + geom.getTypeName());
            }

            return interaction;
        }

        public void StartGrasp()
        {
            if (interaction == null) return;

            Imstk.Geometry rigidGeometry = rigidModel.GetDynamicObject().getCollidingGeometry();
            Imstk.AnalyticalGeometry analyticalGraspingGeom;
            if (graspingGeometry == null)
            {
                analyticalGraspingGeom = Imstk.Utils.CastTo<Imstk.AnalyticalGeometry>(rigidGeometry);
            }
            else
            {
                // Transform only needs to be set for the start of grasp as the grasping constraint
                // is against the rigid body and will therefore move automatically
                analyticalGraspingGeom = Imstk.Utils.CastTo<Imstk.AnalyticalGeometry>(graspingGeometry.GetOutputGeometry());

                // Imstk Geometry has a "local" transform that can be set by geom.setPosition and geom.setOrientation
                // this is incompatible with the unity transform hierarchy. If the geometry is under the rigid, and 
                // _not_ using position and orientation, use the localToWorld transform, otherwise _just_ use the rigid's
                // transform and assume that the local orientation wrt to the rigid is set correctly in the editor
                if (useUnityTransform)
                {
                    analyticalGraspingGeom.setTransform(graspingGeometry.transform.localToWorldMatrix.ToMat4d());
                } else
                {
                    analyticalGraspingGeom.setTransform(rigidGeometry.getTransform());
                    analyticalGraspingGeom.updatePostTransformData();
                }
            }

            if (analyticalGraspingGeom == null)
            {
                Debug.LogError("Grasping Geometry can't be null in " + gameObject.name);
                return;
            }

            switch (graspType)
            {
                case (GraspType.Cell):
                    interaction.beginCellGrasp(analyticalGraspingGeom);
                    break;
                case (GraspType.Vertex):
                    interaction.beginVertexGrasp(analyticalGraspingGeom);
                    break;
            }

            _imstkCollision = rigidModel.GetCollisionInteractionWith(graspedObject);
        }

        private void Update()
        {
            // Keyboard control
            if (useKeyboardControl)
            {
                if (Input.GetKeyDown(graspKey))
                {
                    StartGrasp();
                }
                else if (Input.GetKeyUp(graspKey))
                {
                    EndGrasp();
                }
            }

            // TODO Refactor, really don't want to do this every frame
            if (interaction != null && disableCollision && _imstkCollision != null)
            {
                if (interaction.hasConstraints())
                {
                    _imstkCollision.setEnabled(false);
                }
                else
                {
                    _imstkCollision.setEnabled(true);
                }
            }
        }

        public void Regrasp()
        {
            interaction?.regrasp();
        }

        public void EndGrasp()
        {
            interaction?.endGrasp();
        }

        /// <summary>
        /// Will return true whenever constraints where generated, this means
        /// that something has _actually_ been grasped.
        /// NOTE it takes at least one simulation manager FixedUpdate() for this
        /// to return a correct value, check in the next frame after calling 
        /// StartGrasp()
        /// </summary>
        public bool HasConstraints()
        {
            return interaction != null && interaction.hasConstraints();
        }
    }

}
