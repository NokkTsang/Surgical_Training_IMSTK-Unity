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
    /// High Level interaction that implements suturing. Needs a needle, thread and tissue.
    /// It will deal with the needle and the tissue collision as well as the needle puncturing
    /// the tissue and the thread being pulled through the tissue.
    /// Notes: currently the constraints generated in imstk for the needle/tissue and thread/tissue
    /// punctures are only one way. This means that no forces will be exterted on the needle as 
    /// it violates the constraint.
    /// Additionally there is an assumption in ismtk at the moment that the tip of the needle is
    /// that _back end_ of the needle mesh.
    /// </summary>
    public class Suturing : ImstkBehaviour
    {
        public Rigid needle;
        public Deformable thread;
        public Deformable tissue;
        public string activationKey = "s";
        public Quest3InputHandler inputHandler;
        private bool _activated = false;
        private bool _triggerWasPressed = false;

        public float needleSurfaceStiffness = 0.5f;
        public float threadSurfaceStiffness = 0.5f;
        public float punctureDotThreshold = 0.8f;

        Imstk.NeedleInteraction _needleInteraction;

        protected override void OnImstkInit()
        {
            if (needle == null)
            {
                Debug.LogError("Needle is null");
                return;
            }
            if (thread == null)
            {
                Debug.LogError("Thread is null");
                return;
            }
            if (tissue == null)
            {
                Debug.LogError("Tissue is null");
                return;
            }

            // Disable if any of the objects are not active
            if (!needle.isActiveAndEnabled || !thread.isActiveAndEnabled || !tissue.isActiveAndEnabled)
            {
                enabled = false;
                return;
            }

            // Make sure everything is initialized
            needle.ImstkInit();
            thread.ImstkInit();
            tissue.ImstkInit();

            var needlePbd = Imstk.Utils.CastTo<Imstk.PbdObject>(needle.GetDynamicObject());
            var threadPbd = Imstk.Utils.CastTo<Imstk.PbdObject>(thread.GetDynamicObject());
            var tissuePbd = Imstk.Utils.CastTo<Imstk.PbdObject>(tissue.GetDynamicObject());

            if (needlePbd == null || threadPbd == null || tissuePbd == null)
            {
                Debug.LogError("Needle, Thread or Tissue do not have a dynamic Object");
                enabled = false;
                return;
            }

            _needleInteraction = new Imstk.NeedleInteraction(tissuePbd, needlePbd, threadPbd);

            var ch = Imstk.Utils.CastTo<Imstk.NeedlePbdCH>(_needleInteraction.getCollisionHandlingAB());
            if (ch == null)
            {
                Debug.LogError("Needle interaction does not have a NeedlePbdCH");
                enabled = false;
                return;
            }
            
            // Initialize thread connection to needle
            ch.init(threadPbd);
            ch.setNeedleToSurfaceStiffness(needleSurfaceStiffness);
            ch.setSurfaceToNeedleStiffness(needleSurfaceStiffness);
            ch.setThreadToSurfaceStiffness(threadSurfaceStiffness);
            ch.setSurfaceToThreadStiffness(threadSurfaceStiffness);
            ch.setPunctureDotThreshold(punctureDotThreshold);

            // Add the needle-tissue interaction to the scene (handles collision)
            SimulationManager.sceneManager.getActiveScene().addInteraction(_needleInteraction);
        }

        public void Pull()
        {
            _needleInteraction.stitch();
        }

        public void Update()
        {
            // Check keyboard key (backward compatibility)
            if (!_activated && Input.GetKeyDown(activationKey))
            {
                Pull();
            }

            // Check VR controller trigger
            if (inputHandler != null && inputHandler.IsTriggerPressed() && !_triggerWasPressed)
            {
                Pull();
                _triggerWasPressed = true;
            }
            else if (inputHandler != null && !inputHandler.IsTriggerPressed())
            {
                _triggerWasPressed = false;
            }
        }
        public Imstk.NeedlePbdCH.PunctureData GetPunctureData()
        {
            return _needleInteraction.getPunctureData();
        }
    }
}
