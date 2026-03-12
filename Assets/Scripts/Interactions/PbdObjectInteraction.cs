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
    // \todo: Deprecate this
    [AddComponentMenu("iMSTK/PbdObjectInteraction")]
    class PbdObjectInteraction : CollisionInteraction
    {
        public override Imstk.SceneObject GetImstkInteraction()
        {
            if (model1 == null || model2 == null) { 
                Debug.LogError("Both models need to be set for interaction on " + gameObject.name);
                return null;
            }

            string cdTypeName = collisionTypeName.ToString();
            if (cdTypeName == StandardCollisionTypes.Auto.ToString())
            {
                cdTypeName = CollisionInteraction.GetCDType(model1, model2);
                if (cdTypeName == "")
                {
                    return null;
                }
            }

            Deformable pbdModel;
            if (model1 as Deformable != null)
            {
                pbdModel = model1 as Deformable;
            }
            else
            {
                pbdModel = model2 as Deformable;
                model2 = model1;
            }

            if (pbdModel == null)
            {
                Debug.LogError("One of the DynamicObjects has to be a PbdModel");
                return null;
            }

            Imstk.PbdObjectCollision collision = null;
            if (model2 is StaticModel)
            {
                collision =
                   new Imstk.PbdObjectCollision(
                       pbdModel.GetDynamicObject() as Imstk.PbdObject,
                       model2.GetDynamicObject(),
                       cdTypeName);
                collision.setDeformableStiffnessA(deformableStiffness1);
                collision.setDeformableStiffnessB(deformableStiffness2);
                collision.setRigidBodyCompliance(rigidBodyCompliance);
            }
            else if (model2 is Deformable)
            {
                collision =
                    new Imstk.PbdObjectCollision(
                        pbdModel.GetDynamicObject() as Imstk.PbdObject,
                        model2.GetDynamicObject() as Imstk.PbdObject,
                        cdTypeName);
                collision.setDeformableStiffnessA(deformableStiffness1);
                collision.setDeformableStiffnessB(deformableStiffness2);
                collision.setRigidBodyCompliance(rigidBodyCompliance);
            }
            else if (model2 is Rigid)
            {
                collision =
                   new Imstk.PbdObjectCollision(
                       pbdModel.GetDynamicObject() as Imstk.PbdObject,
                       model2.GetDynamicObject(),
                       cdTypeName);
                collision.setDeformableStiffnessA(deformableStiffness1);
                collision.setDeformableStiffnessB(deformableStiffness2);
                collision.setRigidBodyCompliance(rigidBodyCompliance);
            }
            else
            {
                Debug.LogWarning("Could not find interaction for objects " + pbdModel.gameObject + " & " + model2.gameObject);
                return null;
            }

            collision.setFriction(friction);
            collision.setRestitution(restitution);
            return collision;
        }
    }
}