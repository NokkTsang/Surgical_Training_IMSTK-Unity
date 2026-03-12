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
    /// Tracking Device that's driven by a vrpn device
    /// </summary>
    [AddComponentMenu("iMSTK/VrpnDevice")]
    public class VrpnDevice : TrackingDevice
    {
        [Tooltip("Use the name from the vrpn.cfg file")]
        public string Name = "Tracker0";
        private int _type = 0;
        public int Type { get { return _type; } }

        [Tooltip("Tracke an analog value (e.g. arduino)")]
        public bool TrackAnalog = false;
        public bool TrackButtons = false;
        public bool TrackPosition = true;

        public ImstkUnity.VrpnDeviceManager manager;

        public void Awake()
        {
            if (TrackAnalog) _type |= 0x1;
            if (TrackButtons) _type |= 0x2;
            if (TrackPosition) _type |= 0x4;
        }

        public void Start()
        {
            GetDevice();
        }

        protected override Imstk.DeviceClient MakeDevice()
        {
            return manager.MakeDeviceClient(this);
        }
    }
}
