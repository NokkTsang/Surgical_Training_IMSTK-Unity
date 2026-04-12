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
    /// Replaces UnityDrivenDevice with grip-gated "clutch" control for the needle.
    /// 
    /// Behaviour:
    ///   - Grip HELD: controller position/rotation drives the needle normally.
    ///     On initial grip press, the offset between the controller and the
    ///     needle's last frozen position is captured, so the needle doesn't
    ///     teleport to the controller — it continues from where it was.
    ///   - Grip RELEASED: the needle freezes in place (last known position/rotation
    ///     remains). The user can reposition their hand freely.
    ///   - Trigger: reserved for stitch (handled by Suturing.cs).
    ///
    /// Setup:
    ///   1. On "Right Controller" GameObject, DISABLE the UnityDrivenDevice component.
    ///   2. Add this script to "Right Controller" instead.
    ///   3. Assign the Quest3InputHandler reference.
    ///   4. On the Needle's RigidController, set the device to this script's game object
    ///      (it inherits from TrackingDevice, so it's compatible).
    /// </summary>
    public class GripGatedNeedleController : TrackingDevice
    {
        [Header("Input")]
        public Quest3InputHandler inputHandler;

        [Header("Debug")]
        public bool enableDebugLog = false;

        private Imstk.DummyClient _device;

        // Frozen position/rotation (what the DummyClient holds when grip is released)
        private Vector3 _frozenPosition;
        private Quaternion _frozenRotation = Quaternion.identity;

        // Clutch offset: difference between controller and frozen needle at moment of grip press
        private Vector3 _grabOffsetPosition;
        private Quaternion _grabOffsetRotation = Quaternion.identity;

        private bool _wasGripping = false;
        private bool _initialized = false;

        /// <summary>
        /// Called by TrackingDevice.GetDevice() to create the underlying iMSTK device.
        /// </summary>
        protected override Imstk.DeviceClient MakeDevice()
        {
            if (_device == null)
            {
                _device = new Imstk.DummyClient();
            }
            return _device;
        }

        void Start()
        {
            // Initialize frozen position to the controller's current transform
            _frozenPosition = transform.position;
            _frozenRotation = transform.rotation;

            if (inputHandler == null)
            {
                inputHandler = GetComponent<Quest3InputHandler>();
                if (inputHandler == null)
                    Debug.LogWarning("[GripGatedNeedleController] No Quest3InputHandler assigned.");
            }
        }

        /// <summary>
        /// Override TrackingDevice.Update() — we control when the device position updates.
        /// </summary>
        new void Update()
        {
            if (_device == null) return;

            bool gripping = (inputHandler != null) ? inputHandler.IsGripPressed() : false;

            if (gripping)
            {
                if (!_wasGripping)
                {
                    // Just grabbed — capture offset so needle doesn't teleport
                    _grabOffsetPosition = transform.position - _frozenPosition;
                    _grabOffsetRotation = Quaternion.Inverse(_frozenRotation) * transform.rotation;

                    if (enableDebugLog)
                        Debug.Log($"[GripGatedNeedle] Grip ON — clutch engaged, offset={_grabOffsetPosition}");
                }

                // While gripping: apply controller movement relative to grab offset
                Vector3 targetPos = transform.position - _grabOffsetPosition;
                Quaternion targetRot = transform.rotation * Quaternion.Inverse(_grabOffsetRotation);

                _frozenPosition = targetPos;
                _frozenRotation = targetRot;
            }
            else
            {
                if (_wasGripping && enableDebugLog)
                    Debug.Log("[GripGatedNeedle] Grip OFF — needle frozen");
            }

            _wasGripping = gripping;

            // Always push the frozen (or currently-tracking) position to the iMSTK device
            _device.setPosition(_frozenPosition.ToImstkVec());
            _device.setOrientation(_frozenRotation.ToImstkQuat());

            if (!_initialized)
            {
                _initialized = true;
                if (enableDebugLog)
                    Debug.Log($"[GripGatedNeedle] Initialized at pos={_frozenPosition}");
            }
        }
    }
}
