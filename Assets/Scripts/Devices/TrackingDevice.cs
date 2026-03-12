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
    enum ButtonState
    {
        // From Imstk
        BUTTON_RELEASED = 0,
        BUTTON_TOUCHED  = 1,
        BUTTON_UNTOUCHED = 2,
        BUTTON_PRESSED = 3
    }


    public abstract class TrackingDevice : MonoBehaviour
    {
        Imstk.DeviceClient trackingDevice = null;

        public Vector3 GetPosition()
        {
            if (trackingDevice == null)
            {
                GetDevice();
                if (trackingDevice == null)
                    return Vector3.zero;
            }
            Imstk.Vec3d pos = trackingDevice.getPosition();
            return pos.ToUnityVec();
        }
        public Quaternion GetOrientation()
        {
            if (trackingDevice == null)
            {
                GetDevice();
                if (trackingDevice == null)
                    return Quaternion.identity;
            }
            Imstk.Quatd quat = trackingDevice.getOrientation();
            return quat.ToUnityQuat();
        }

        public Vector3 GetForce()
        {
            Imstk.Vec3d vec = trackingDevice.getForce();
            return vec.ToUnityVec();
        }

        public bool IsButtonDown(int id)
        {
            var val = trackingDevice.getButton(id);
            if (val == (int)ButtonState.BUTTON_PRESSED || val == (int)ButtonState.BUTTON_TOUCHED) return true;
            else return false;
        }

        public int[] GetButtons()
        {
            var result = new int[8];

            for (int i = 0; i<8;++i)
            {
                result[i] = trackingDevice.getButton(i);
            }
            var b = trackingDevice.getButtons();
            return result;
        }

        public float GetAnalog(int id)
        {
            var values = trackingDevice.getAnalog();
            if (values.Count > id) return (float)values[id];
            else
            {
                Debug.LogWarning("Id " + id.ToString() + " not found ");
                return 0;
            }
        }

        protected abstract Imstk.DeviceClient MakeDevice();

        public void Update()
        {
            // This is not directly used, but displayed
            Transform transform = gameObject.GetComponentFatal<Transform>();
            transform.SetPositionAndRotation(GetPosition(), GetOrientation());
        }

        public Imstk.DeviceClient GetDevice()
        {
            if (trackingDevice == null)
            {
                trackingDevice = MakeDevice();
                trackingDevice.setButtonsEnabled(true);
            }
            return trackingDevice;
        }
    }
}