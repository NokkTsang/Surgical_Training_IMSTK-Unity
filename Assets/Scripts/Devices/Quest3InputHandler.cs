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
using UnityEngine.XR;

namespace ImstkUnity
{
    /// <summary>
    /// Handler for Quest 3 controller input to simulate device buttons
    /// for suturing and other medical simulation tasks
    /// </summary>
    public class Quest3InputHandler : MonoBehaviour
    {
        [Header("Controller Settings")]
        [Tooltip("Which controller to track (Left or Right)")]
        public XRNode controllerNode = XRNode.RightHand;

        [Header("Input Actions")]
        [Tooltip("Trigger press simulates tool activation (e.g., grasping needle)")]
        public bool triggerPressed = false;
        
        [Tooltip("Grip press simulates secondary action")]
        public bool gripPressed = false;

        private InputDevice targetDevice;

        void Start()
        {
            // Get the target controller device
            targetDevice = InputDevices.GetDeviceAtXRNode(controllerNode);
        }

        void Update()
        {
            // Ensure we have a valid device
            if (!targetDevice.isValid)
            {
                targetDevice = InputDevices.GetDeviceAtXRNode(controllerNode);
                return;
            }

            // Read trigger button state
            if (targetDevice.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerValue))
            {
                triggerPressed = triggerValue;
            }

            // Read grip button state
            if (targetDevice.TryGetFeatureValue(CommonUsages.gripButton, out bool gripValue))
            {
                gripPressed = gripValue;
            }
        }

        /// <summary>
        /// Check if trigger is currently pressed
        /// </summary>
        public bool IsTriggerPressed()
        {
            return triggerPressed;
        }

        /// <summary>
        /// Check if grip is currently pressed
        /// </summary>
        public bool IsGripPressed()
        {
            return gripPressed;
        }

        /// <summary>
        /// Get the controller position
        /// </summary>
        public Vector3 GetControllerPosition()
        {
            if (targetDevice.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position))
            {
                return position;
            }
            return transform.position;
        }

        /// <summary>
        /// Get the controller rotation
        /// </summary>
        public Quaternion GetControllerRotation()
        {
            if (targetDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation))
            {
                return rotation;
            }
            return transform.rotation;
        }
    }
}
