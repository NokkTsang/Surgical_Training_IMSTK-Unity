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
using System.Threading;

namespace ImstkUnity
{
    // HS-2022-feb-02 Should be refactored to remove the "manager" functionality from the
    // Device, see the VRPNDevice for the pattern. Remove `static` components
    // from here

#if IMSTK_USE_OPENHAPTICS
    // If this is not defined iMSTK was not built with Open Haptics enabled
    // to use build iMSTK with the flag iMSTK_USE_OpenHaptics set to ON
    [AddComponentMenu("iMSTK/OpenHapticsDevice")]
    public class OpenHapticsDevice : TrackingDevice
    {
        public string deviceName = "default";

        public static Imstk.OpenHapticDeviceManager openHapticDeviceManager = null;
        public static bool hapticsRunning = false;
        public static Thread hapticThread = null;

        public static void InitManager()
        {
            if (openHapticDeviceManager != null)
            {
                openHapticDeviceManager.init();
            }
        }

        public static void StartManager()
        {
            // Launch haptics on a separate thread if using
            if (openHapticDeviceManager != null)
            {
                hapticsRunning = true;
                Debug.Log("OpenHaptics Thread Starting");
                hapticThread = new Thread(() =>
                {
                    while (hapticsRunning)
                    {
                        if (openHapticDeviceManager != null)
                        {
                            openHapticDeviceManager.update();
                        }
                    }
                });
                hapticThread.Start();
            }
        }
        public static void StopManager()
        {
            if (openHapticDeviceManager != null)
            {
                hapticsRunning = false;
                hapticThread.Join();

                Debug.Log("OpenHaptics Thread Stopping");
                openHapticDeviceManager.uninit();
                openHapticDeviceManager = null;
                hapticThread = null;
            }
        }

        protected override Imstk.DeviceClient MakeDevice()
        {
            if (openHapticDeviceManager == null)
            {
                openHapticDeviceManager = new Imstk.OpenHapticDeviceManager();
            }
            Debug.Log("MakeDeviceClient <" + deviceName + ">");
            // Creates the default device (specify name for specific one)
            if (deviceName == "default")
            {
                return openHapticDeviceManager.makeDeviceClient();
            }
            else
            {
                return openHapticDeviceManager.makeDeviceClient(deviceName);
            }
        }
    }
#else
    public class OpenHapticsDevice : TrackingDevice
    {
        public string deviceName = "default";

        public static void InitManager()
        {
            Debug.LogError("OpenHaptics is not enable in this build");
        }

        public void Start() {}

        public static void StartManager() {}
        public static void StopManager() {}

        protected override Imstk.DeviceClient MakeDevice() {
            Debug.LogError("OpenHaptics is not enable in this build, " +
                "see the documentation for more information ");
            return null; 
        }
    }
#endif
}