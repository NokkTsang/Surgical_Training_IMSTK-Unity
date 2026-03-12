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
using UnityEngine;

namespace ImstkUnity
{
    // Still need to manually maintain this list if users want to 
    // Select an explicit 
    public enum StandardCollisionTypes
    {
        Auto,
        BidirectionalPlaneToSphereCD,
        ImplicitGeometryToPointSetCCD,
        ImplicitGeometryToPointSetCD,
        MeshToMeshBruteForceCD,
        PointSetToCapsuleCD,
        PointSetToOrientedBoxCD,
        PointSetToPlaneCD,
        PointSetToSphereCD,
        SphereToCylinderCD,
        SphereToSphereCD,
        SurfaceMeshToCapsuleCD,
        SurfaceMeshToSphereCD,
        SurfaceMeshToSurfaceMeshCD,
        TetraToLineMeshCD,
        TetraToPointSetCD,
        UnidirectionalPlaneToSphereCD
    };



    /// <summary>
    /// Convience collision interaction, uses a lot of defaults. If specifics needed
    /// use the specific subclasses
    /// </summary>
    [AddComponentMenu("iMSTK/CollisionInteraction")]
    public class CollisionInteraction : ImstkInteractionBehaviour
    {
        [Obsolete("The collisionType parameter should not be used anymore")]
        public StandardCollisionTypes collisionType = StandardCollisionTypes.Auto;
        
        public string collisionTypeName = "Auto";
        public DynamicalModel model1;
        public DynamicalModel model2;

        Imstk.CollisionInteraction interaction;

        public double friction = 0.0;
        public double restitution = 0.0;

        [Tooltip("A good default value is 1/number of iterations from the simulation manager")]
        public double deformableStiffness1 = 0.2;
        [Tooltip("A good default value is 1/number of iterations from the simulation manager")]
        public double deformableStiffness2 = 0.2;
        public double rigidBodyCompliance = 0.0001;

        /// <summary>
        /// Start intentionally empty, including this enables the "enabled" check-box in the 
        /// editor, allowing the activation/deactivation of this component in the GUI
        /// </summary>
        void Start()
        {

        }

        // Create the Imstk interaction for this collision interaction
        public override Imstk.SceneObject GetImstkInteraction()
        {
            if (interaction != null) return interaction;
            if (model2 == null || model1 == null)
            {
                Debug.LogWarning("Interaction on object " + gameObject.name + " which does not have a DynamicalModel");
                return null;
            }

            if (!model1.isActiveAndEnabled || !model2.isActiveAndEnabled || !isActiveAndEnabled)
            {
                return null;
            }

            if (collisionTypeName == "Auto" || collisionTypeName == "")
            {
                collisionTypeName = GetCDType(model1, model2);
                if (collisionTypeName == "")
                {
                    return null;
                }
                Debug.Log("Using " + collisionTypeName + " for " + ImstkUnity.Utility.GetFullName(model1) + " and " +
                    ImstkUnity.Utility.GetFullName(model2));
            }

            // Right now just check for Rigid and Deformable
            if (model1 is Rigid || model1 is Deformable)
            {
                interaction = CreateInteraction(model1, model2);
            }
            else if (model2 is Rigid || model2 is Deformable)
            {
                interaction = CreateInteraction(model2, model1);
            }

            if (interaction == null)
            {
                Debug.LogWarning("Creating collision between two unsupported types, at least one needs to be a Rigd or Deformable");
            }

            return interaction;
        }
        /// <summary>
        /// Look up the collision detection type in the iMSTK factory 
        /// </summary>
        /// <returns>A string with the deduced collision type, "" otherwise</returns>
        public static string GetCDType(DynamicalModel model1, DynamicalModel model2)
        {
            string result = "";

            if (model1 == null || model2 == null)
            {
                //Debug.LogWarning("Can't determine collision one of the models is null");
                return result;
            }

            Imstk.Geometry geom1 = model1.GetCollidingGeometry();
            Imstk.Geometry geom2 = model2.GetCollidingGeometry();

            if (geom1 == null || geom2 == null)
            {
                //Debug.LogWarning("Can't determine collision one of the models collision geometry is null");
                return result;
            }

            result = Imstk.CDObjectFactory.getCDType(geom1, geom2);

            return result;
        }

        private Imstk.CollisionInteraction CreateInteraction(DynamicalModel model1, DynamicalModel model2)
        {
            var pbd = Imstk.Utils.CastTo<Imstk.PbdObject>(model1.GetDynamicObject());
            Debug.Assert(pbd != null);
            // At the moment all colliding objects are PBD 
            // PbdObjectCollision expects first parameter to be PBD, the second parameter
            // may be any type of CollidingObj
            Imstk.PbdObjectCollision collision =
                new Imstk.PbdObjectCollision(
                    pbd,
                    model2.GetDynamicObject(),
                    collisionTypeName);

            // Just configure ALL the parameters
            collision.setDeformableStiffnessA(deformableStiffness1);
            collision.setDeformableStiffnessB(deformableStiffness2);
            collision.setRigidBodyCompliance(rigidBodyCompliance);
            collision.setFriction(friction);
            collision.setRestitution(restitution);
            collision.setUseCorrectVelocity(false);

            Debug.Log("Created collision interaction of type: " + collision.GetType().Name + "for objects " +
                model1.gameObject.name + " & " + model2.gameObject.name);

            return collision;
        }

    }
}
