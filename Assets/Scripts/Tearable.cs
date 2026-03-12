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
    /// Makes a deformable object tearable
    /// </summary>
    /// Currently this is only well tested in iMSTK with Line Meshes
    /// Notes: Tearable is the first iMSTK Component figure out interfaces
    /// Currently PbdObject is still a SceneObject (which is an entity)
    /// This means we need to have it initialized before we can add ourselves
    /// to it
    public class Tearable : ImstkBehaviour
    {
        Imstk.Tearable _tearable;
        public float maxStrain = 1f;

        protected override void OnImstkComponentInit()
        {
            _tearable = new Imstk.Tearable();
            _tearable.setMaxStrain((double)maxStrain);

            var pbdObject = FindDeformable();
            if (pbdObject != null)
            {
                pbdObject.addComponent(Imstk.Utils.CastTo<Imstk.Component>(_tearable));
                Debug.Log("Tearable added to PbdObject " + pbdObject.getName());
            }
            else
            {
                Debug.LogWarning("Could not find a PbdObject on the current gameobject, tearable not initialized");
            }
        }

        private Imstk.PbdObject FindDeformable()
        {
            var deformable = GetComponent<DynamicalModel>();
            if (deformable != null)
            {
                deformable.dynamicGeometry = true;
                return Imstk.Utils.CastTo<Imstk.PbdObject>(deformable.GetDynamicObject());
            }
            return null;
        }
    }
}