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
using UnityEngine.Assertions;

namespace ImstkUnity
{
    /// <summary>
    /// Object to implement switching between two objects controlled by a rigid controller (e.g. tools) 
    /// this can be used to implement a tool switch in the simulation
    /// </summary>
    public class SwitchControlledObject : MonoBehaviour
    {
        public RigidController rigidController;

        public List<GameObject> objects = new List<GameObject>();
        public int initialObject = 0;
        public string switchKey = " ";

        struct ObjectState
        {
            public GameObject gameObject;
            public Rigid rigid;
            public Imstk.PbdBody imstkPbdBody;
            public bool ignoreGravity;
            public Dictionary<Imstk.CollisionInteraction, bool> collisionStates;
        }

        private List<ObjectState> _objectStates = new List<ObjectState>();

        int _selectedObject = 0;


        // Update is called once per frame
        void Update()
        {
            // We need the imstk objects to be initialized this depends 
            // on simulation manager call order ...
            if (_objectStates.Count == 0)
            {
                foreach (var item in objects)
                {
                    if (item == null || !item.activeSelf)
                        continue;
                    _objectStates.Add(GetStateFor(item));
                    Deactivate(_objectStates[_objectStates.Count - 1]);
                }

                Activate(_objectStates[initialObject]);
                _selectedObject = initialObject;
            }

            if (Input.GetKeyDown(switchKey))
            {
                Activate((_selectedObject + 1) % _objectStates.Count);
            }
        }

        /// <summary>
        /// Switch the selected object
        /// </summary>
        /// <param name="objectIndex">Index of the object that you want to be come active</param>
        public void Activate(int objectIndex)
        {
            if (_selectedObject == objectIndex || objectIndex >= _objectStates.Count)
                return;

            Deactivate(_objectStates[_selectedObject]);

            _selectedObject = objectIndex;

            Activate(_objectStates[_selectedObject]);
        }

        private void Deactivate(ObjectState state)
        {

            // Find all collision interactions with object
            // deactivate them

            // To keep the object from moving around just ignore gravity
            // and set velocities to zero
            // this is to work around that we can't just deactivate a rigid
            // We're also using the imstk object here rather than the unity behavior 
            // as the properties aren't routed through to imstk once the simulation is running
            state.ignoreGravity = state.imstkPbdBody.bodyGravity;
            state.imstkPbdBody.bodyGravity = true;
            state.imstkPbdBody.overrideLinearAndAngularVelocity(new Imstk.Vec3d(0, 0, 0), new Imstk.Vec3d(0, 0, 0));

            // Back up the activate status of the collision interactions and disable
            state.collisionStates.Clear();
            foreach (var collision in state.rigid.collisionInteractions)
            {
                state.collisionStates.Add(collision, collision.getEnabled());
                collision.setEnabled(false);
            }

            state.gameObject.SetActive(false);
        }

        private void Activate(ObjectState state)
        {
            // Set the new active rigid position and orientation to the controller position and orientation
            // Clear out the velocities. This will teleport the new object to the controller position
            // and prevent activation of the controller mass spring system
            state.imstkPbdBody.overrideRigidPositionAndOrientation(rigidController.GetPosition().ToImstkVec(),
                rigidController.GetOrientation().ToImstkQuat());
            state.imstkPbdBody.overrideLinearAndAngularVelocity(new Imstk.Vec3d(0, 0, 0), new Imstk.Vec3d(0, 0, 0));

            // Find all collision interactions with object
            // activate them
            // Restore the state of the collision interactions
            foreach (var collision in state.rigid.collisionInteractions)
            {
                if (state.collisionStates.ContainsKey(collision))
                {
                    collision.setEnabled(state.collisionStates[collision]);
                }
            }

            state.imstkPbdBody.bodyGravity = state.ignoreGravity;

            rigidController.SetControlledObject(state.rigid);

            state.gameObject.SetActive(true);
        }

        private ObjectState GetStateFor(GameObject item)
        {
            var result = new ObjectState();
            result.gameObject = item;

            var rigid = item.GetComponentInChildren<Rigid>();
            Assert.IsNotNull(rigid, "Could not find rigid in " + item.name);
            result.rigid = rigid;

            var pbdObject = Imstk.Utils.CastTo<PbdObject>(rigid.GetDynamicObject());
            Assert.IsNotNull(pbdObject, "Could not find pbdObject in " + item.name);
            result.imstkPbdBody = pbdObject.getPbdBody();

            result.ignoreGravity = rigid.ignoreGravity;

            // Well check the size of the list and the states on deactivate/activate
            result.collisionStates = new Dictionary<Imstk.CollisionInteraction, bool>();

            return result;
        }

    }
}
