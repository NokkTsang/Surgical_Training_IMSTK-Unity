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
    /// Object used between a device handled by the user and a ``Rigid``. 
    /// </summary>
    /// It utilizes a mass spring system to correct for latency in the system.
    /// It corrects for problems with haptics in simulation systems. By manipulating
    /// the spring parameters the haptic response can be tuned to the behavior of 
    /// the computer and the simulated system.
    [AddComponentMenu("iMSTK/RigidController")]
    public class RigidController : ImstkControllerBehaviour
    {
        Imstk.PbdObjectController controller = null;
        public Rigid rigid = null;

        public double angularKd = 50.0;
        public double angularKs = 1000.0;
        public double linearKd = 100.0;
        public double linearKs = 10000.0;
        public bool useCriticalDamping = true;

        public double forceScale = 0.00001;
        public bool useForceSmoothing = true;
        public int forceSmoothingKernelSize = 15;

        // All these transforms below could really just be one
        public Vector3 attachmentPoint = Vector3.zero;
        public Vector3 translationalOffset = Vector3.zero;
        public Quaternion rotationalOffset = Quaternion.identity;
        public Quaternion localRotationalOffset = Quaternion.identity;
        public double translationScaling = 1;

        // Unity uses LHS, while imstk uses RHS, invert X positional
        public bool invertX = false;
        public bool invertY = false;
        public bool invertZ = true;

        // Unity uses LHS, while imstk uses RHS, invert Y,Z planes
        public bool invertRotX = true;
        public bool invertRotY = true;
        public bool invertRotZ = false;

        public bool debugController = false;

#if UNITY_EDITOR
        // These are used to drive the editor foldouts
        public bool _forceFoldout = false;
        public bool _offsetFoldout = false;
        public bool _axisMappingFoldout = false;
#endif

        public Vector3 GetPosition()
        {
            Imstk.Vec3d pos = controller.getPosition();
            return pos.ToUnityVec();
        }
        public Quaternion GetOrientation()
        {
            Imstk.Quatd quat = controller.getOrientation();
            return quat.ToUnityQuat();
        }

        public override Imstk.DeviceControl GetController()
        {
            if (device == null)
            {
                Debug.LogError("Failed to create controller, no device given");
                return null;
            }
            if (rigid == null)
            {
                Debug.LogError("Failed to create controller, no controlled object given");
                return null;
            }

            if (controller != null)
            {
                return controller;
            }

            controller = new Imstk.PbdObjectController(gameObject.name);
            controller.setControlledObject(rigid.GetDynamicObject());
            controller.setDevice(device.GetDevice());
            controller.setAngularKd(angularKd);
            controller.setAngularKs(angularKs);
            controller.setLinearKd(linearKd);
            controller.setLinearKs(linearKs);

            controller.setUseCritDamping(useCriticalDamping);

            controller.setForceScaling(forceScale);
            controller.setUseForceSmoothening(useForceSmoothing);
            controller.setSmoothingKernelSize(forceSmoothingKernelSize);

            controller.setHapticOffset(attachmentPoint.ToImstkVec());

            ;
            controller.setTranslationOffset((translationalOffset + transform.TransformDirection(attachmentPoint)).ToImstkVec());
            controller.setRotationOffset(rotationalOffset.ToImstkQuat());
            controller.setEffectorRotationOffset(localRotationalOffset.ToImstkQuat());
            controller.setTranslationScaling(translationScaling);

            Imstk.TrackingDeviceControl.InvertFlag invertFlag = 0x00;
            if (invertX)
            {
                invertFlag = invertFlag | Imstk.TrackingDeviceControl.InvertFlag.transX;
            }
            if (invertY)
            {
                invertFlag = invertFlag | Imstk.TrackingDeviceControl.InvertFlag.transY;
            }
            if (invertZ)
            {
                invertFlag = invertFlag | Imstk.TrackingDeviceControl.InvertFlag.transZ;
            }
            if (invertRotX)
            {
                invertFlag = invertFlag | Imstk.TrackingDeviceControl.InvertFlag.rotX;
            }
            if (invertRotY)
            {
                invertFlag = invertFlag | Imstk.TrackingDeviceControl.InvertFlag.rotY;
            }
            if (invertRotZ)
            {
                invertFlag = invertFlag | Imstk.TrackingDeviceControl.InvertFlag.rotZ;
            }

            controller.setInversionFlags((byte)invertFlag);

            return controller;
        }

        public void SetControlledObject(Rigid rigid)
        {
            var pbdObject = Imstk.Utils.CastTo<Imstk.PbdObject>(rigid.GetDynamicObject());
            controller.setControlledObject(pbdObject);
        }

#if UNITY_EDITOR
        // Leaving this on in the built version causes unexpected behavior
        public void Update()
        {
            if (debugController)
            {
                gameObject.transform.SetPositionAndRotation(GetPosition(), GetOrientation());
            }
        }
#endif
    }
}