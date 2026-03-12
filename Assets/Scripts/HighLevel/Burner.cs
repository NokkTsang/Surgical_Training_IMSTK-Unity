
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
using System.Collections.Generic;
using ImstkUnity;

namespace ImstkUnity
{
    /// <summary>
    /// Class to implement a tool that applies power to burnable objects (e.g. monopolar device)
    /// You can register burnable objects with the burner and then turn it on and off, on contact
    /// power will be applied to the burnable objects and they will destroy elements on prolonged 
    /// contact
    /// </summary>
    public class Burner : ImstkBehaviour
    {
        Imstk.Burner _burner;
        public ImstkUnity.Rigid tool;
        public double wattage = 80;
        public double onTime = 1.0;

        public GeometryFilter burningGeometry;
        public List<Burnable> burnables;

        public TrackingDevice device;
        public int burnButton = 0;
        public string burnKey = "b";

        private Latch _keyLatch;
        private Latch _deviceLatch;

        private void Awake()
        {
            _keyLatch = new Latch(() => Input.GetKey(burnKey), () => StartBurning(), () => StopBurning());
            _deviceLatch = new Latch(() => device != null && device.IsButtonDown(burnButton), () => StartBurning(), () => StopBurning());
        }

        protected override void OnImstkComponentInit()
        {
            _burner = new Imstk.Burner();
            if (tool == null || !tool.isActiveAndEnabled)
            {
                enabled = false;
                return;
            }

            var pbdObject = Imstk.Utils.CastTo<Imstk.PbdObject>(tool.GetDynamicObject());
            pbdObject.addComponent(Imstk.Utils.CastTo<Imstk.Component>(_burner));
                
            if (burningGeometry != null)
            {
                var imstkGeom = Imstk.Utils.CastTo<Imstk.AnalyticalGeometry>(burningGeometry.GetOutputGeometry());
                if (imstkGeom != null)
                {
                    _burner.setBurnerGeometry(imstkGeom);
                    tool.Attach(imstkGeom);
                }
                else
                {
                    Debug.LogWarning("Burner Geometry needs to be AnalyticalGeometry");
                }
            }
        }

        protected override void OnImstkStart()
        {
            foreach (var burnable in burnables)
            {
                if (burnable != null && burnable.isActiveAndEnabled)
                {
                    Debug.LogWarning($"{burnable.name} Added to burner");
                    _burner.addObject(burnable.GetPbdObject());
                }
            }
        }

        public void Update()
        {
            _keyLatch.Update();
            _deviceLatch.Update();
        }

        public void StartBurning()
        {
            if (!_burner.getState())
            {
                Debug.Log("Starting burner");
                _burner.setWattage(wattage);
                _burner.setOnTime(onTime);
                _burner.start();
            }
        }

        public void StopBurning()
        {
            if (_burner.getState())
            {
                Debug.Log("Stopping burner");
                _burner.stop();
            }
        }

        public bool IsOn()
        {
            return _burner.getState();
        }

        public bool DidBurn()
        {
            return _burner.getDidBurn();
        }
    }
}
