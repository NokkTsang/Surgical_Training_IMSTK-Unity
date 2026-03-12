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

    public interface ITwoPartTool
    {
        public abstract float GetOpenValue();
    }

    /// <summary>
    /// GraspingController takes care of some of the higher level aspects of 
    /// grasping and tool operation, it can manipulate two jaws and open and close 
    /// them, it can take input from a variety of sources as well
    /// </summary>
    [RequireComponent(typeof(GraspingManager))]
    public class GraspingController : MonoBehaviour, ITwoPartTool
    {
        // Assumes that the jaws are currently closed, will open
        // to the max opening angle
        // Rotation is currently just fixed to the Z Axis
        public TrackingDevice device;

        // TODO disable collision on grasp

        /// Drive via Buttons
        public int openButton = 0;
        public int closeButton = 1;

        // Drive via Keys
        public string openKey = "]";
        public string closeKey = "[";

        public bool useSnap = false;
        public float transitionTime = 3.0f;
        private float openValue = 0.0f;

        /// Drive by analog Value
        public bool driveWithAnalog = false;
        public int analogChannel = 0;
        public float analogClosed = 0;
        public float analogOpen = 1024;

        public GameObject upperJaw;
        public GameObject lowerJaw;


        [Tooltip("The largest angle between the two jaws")]
        public float maxOpenAngle;

        GraspingManager _manager;
        public GraspingManager manager { get { return _manager; } }


        private float _lowerCloseAngle = 0.0f;
        private float _upperCloseAngle = 0.0f;

        public float GetOpenValue() { return openValue; }

        void Start()
        {
            if (lowerJaw != null)
            {
                _lowerCloseAngle = lowerJaw.transform.localRotation.eulerAngles.z;
            }
            if (upperJaw != null)
            {
                _upperCloseAngle = upperJaw.transform.localRotation.eulerAngles.z;
            }

            if (!device.isActiveAndEnabled)
            {
                device = null;
            }

            // need to check startup order to see if we can initialize the imstk values at this point
            // we'd want to do the type check and casting here rather than every frame

            _manager = GetComponent<GraspingManager>();
        }

        // Update is called once per frame
        void Update()
        {
            float newValue = openValue;
            if (device == null) return;

            if (driveWithAnalog)
            {
                float input = device.GetAnalog(analogChannel);
#pragma warning disable 1718 // Warning for comparing the same variable
                if (input == input)
                {
                    if (analogClosed < analogOpen)
                    {
                        newValue = (input - analogClosed) / (analogOpen - analogClosed);
                    }
                    else
                    {
                        newValue = (analogClosed - input) / (analogClosed - analogOpen);
                    }
                }
#if UNITY_EDITOR
                else
                {
                    Debug.LogWarning("Can't read analog from channel: " + analogChannel);
                }
#endif
#pragma warning restore 1718

                //Debug.Log(Time.realtimeSinceStartup-start + " " + input.ToString());
            }
            else
            {
                if (useSnap)
                {
                    if (device.IsButtonDown(openButton) || Input.GetKey(openKey)) newValue = 0.0f;
                    if (device.IsButtonDown(closeButton) || Input.GetKey(closeKey)) newValue = 1.0f;
                }
                else
                {
                    // Map buttons to axis, this should probably be in the input _manager
                    if (device.IsButtonDown(openButton) || Input.GetKey(openKey)) newValue = openValue + Time.deltaTime / transitionTime;
                    if (device.IsButtonDown(closeButton) || Input.GetKey(closeKey)) newValue = openValue - Time.deltaTime / transitionTime;
                }
            }



            var oldClosed = isClosed();

            SetOpenValue(Mathf.Clamp01(newValue));

            if (isClosed() && !oldClosed)
            {
                Debug.Log("Start Grasp");
                _manager.StartGrasp();
            }

            if (!isClosed() && oldClosed)
            {
                _manager.EndGrasp();
            }
        }

        bool isClosed()
        {
            return openValue < 0.1;
        }

        void SetOpenValue(float value)
        {
            float half = (maxOpenAngle * value) / 2.0f;

            if (upperJaw != null)
            {
                var rot = upperJaw.transform.localRotation;
                var angles = upperJaw.transform.localRotation.eulerAngles;
                angles.z = _upperCloseAngle + half;
                rot.eulerAngles = angles;
                upperJaw.transform.localRotation = rot;
            }

            if (lowerJaw != null)
            {
                var rot = lowerJaw.transform.localRotation;
                var angles = lowerJaw.transform.localRotation.eulerAngles;
                angles.z = _lowerCloseAngle - half;
                rot.eulerAngles = angles;
                lowerJaw.transform.localRotation = rot;
            }
            openValue = value;
        }
    }
}

