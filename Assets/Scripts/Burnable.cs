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
using ImstkUnity;
using System.Collections.Generic;
using UnityEngine;


namespace ImstkUnity
{
    /// <summary>
    /// Makes a deformable object burnable
    /// </summary>
    /// Currently this is only well tested in iMSTK with Line Meshes
    /// Notes: Burnable is an early iMSTK Component figure out interfaces
    public class Burnable : ImstkBehaviour
    {
        Imstk.Burnable _burnable;

        public bool trackOnly = false;

        public PbdObjectCellRemoval.OtherMeshUpdateType updateType = PbdObjectCellRemoval.OtherMeshUpdateType.CollisionAndVisualReused;

        protected override void OnImstkComponentInit()
        {
            _burnable = new Imstk.Burnable();
            _burnable.setTrackOnly(trackOnly);
            _burnable.setUpdateType(updateType);

            var pbdObject = FindDeformable();
            if (pbdObject != null)
            {
                pbdObject.addComponent(Imstk.Utils.CastTo<Imstk.Component>(_burnable));
                Debug.Log("Burnable added to PbdObject " + pbdObject.getName());
            }
            else
            {
                Debug.LogWarning("Could not find a Deformable on the current gameobject, burnable not initialized");
            }
        }

        public Imstk.PbdObject GetPbdObject()
        {
            return _burnable.getPbdObject();
        }

        private Imstk.PbdObject FindDeformable()
        {
            var deformable = GetComponent<DynamicalModel>();
            if (deformable != null)
            {
                if (!trackOnly) deformable.dynamicGeometry = true;
                return Imstk.Utils.CastTo<Imstk.PbdObject>(deformable.GetDynamicObject());
            }
            return null;
        }

        public Imstk.VectorInt GetRemovedCells()
        {
            return _burnable.getCellRemover().getRemovedCells();
        }

        public void Remove(List<int> ids)
        {
            var remover = _burnable.getCellRemover();
            for (int i = 0; i < ids.Count; i++)
            {
                remover.removeCellOnApply(ids[i]);
            }
        }
    }
}