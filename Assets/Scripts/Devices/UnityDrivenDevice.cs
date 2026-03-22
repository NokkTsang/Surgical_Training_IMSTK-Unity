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

namespace ImstkUnity
{
    /// <summary>
    /// This is an example how to use the "DummyDevice" in imstk to connect 
    /// positions and transforms coming from unity and use them to control
    /// an object inside of imstk. This device just reads the position and 
    /// rotation from the current transform of the object that is sitting on
    /// and can be used as the input device in a "RigidObjectController"
    /// Please note that the RigidObjectController implements a mass spring 
    /// system so the position of the controlled rigid will not always be
    /// exactly the input position
    /// </summary>
    public class UnityDrivenDevice : ImstkUnity.TrackingDevice
    {
        Imstk.DummyClient _device;

        // Update is called once per frame it is threadsafe in respect
        // to the imstk update loop
        // As we are pushing the position into the device we're overriding
        // the TrackingDevice update 
        new void Update()
        {
            if (_device == null)
            {
                UnityEngine.Debug.LogWarning("[UnityDrivenDevice] Device not initialized");
                return;
            }

            var pos = transform.position.ToImstkVec();
            var rot = transform.rotation.ToImstkQuat();
            _device.setPosition(pos);
            _device.setOrientation(rot);
        }

        protected override Imstk.DeviceClient MakeDevice()
        {
            if (_device == null)
            {
                _device = new Imstk.DummyClient();
            }
            return _device;
        }
    }
}
